using Microsoft.EntityFrameworkCore;
using QuestionBank.Application.DTOs;
using QuestionBank.Domain.Entities;
using QuestionBank.Domain.Enums;
using QuestionBank.Infrastructure.Data;
using QuestionBank.Infrastructure.Repositories;

namespace QuestionBank.Application.Services;

public class ExamService : IExamService
{
    private readonly IExamRepository _examRepository;
    private readonly IPaperRepository _paperRepository;
    private readonly ApplicationDbContext _context;

    public ExamService(
        IExamRepository examRepository,
        IPaperRepository paperRepository,
        ApplicationDbContext context)
    {
        _examRepository = examRepository;
        _paperRepository = paperRepository;
        _context = context;
    }

    public async Task<ApiResponse<ExamDto>> GetExamByIdAsync(Guid id)
    {
        var exam = await _examRepository.GetExamWithPaperAsync(id);
        if (exam == null)
        {
            return ApiResponse<ExamDto>.ErrorResponse("考试不存在");
        }

        return ApiResponse<ExamDto>.SuccessResponse(MapToExamDto(exam));
    }

    public async Task<ApiResponse<List<ExamDto>>> GetAllExamsAsync()
    {
        var exams = await _context.Exams
            .Include(e => e.Paper)
            .Include(e => e.Creator)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<ExamDto>>.SuccessResponse(
            exams.Select(MapToExamDto).ToList());
    }

    public async Task<ApiResponse<ExamDto>> CreateExamAsync(CreateExamDto createDto, Guid creatorId)
    {
        // 验证试卷是否存在
        var paper = await _paperRepository.GetByIdAsync(createDto.PaperId);
        if (paper == null)
        {
            return ApiResponse<ExamDto>.ErrorResponse("试卷不存在");
        }

        var exam = new Exam
        {
            Title = createDto.Title,
            Description = createDto.Description,
            PaperId = createDto.PaperId,
            CreatorId = creatorId,
            Status = ExamStatus.Draft,
            StartTime = createDto.StartTime,
            EndTime = createDto.EndTime,
            Duration = createDto.Duration,
            MaxAttempts = createDto.MaxAttempts,
            AnswerDisplayMode = createDto.AnswerDisplayMode,
            AllowPause = createDto.AllowPause,
            ShuffleQuestions = createDto.ShuffleQuestions,
            ShuffleOptions = createDto.ShuffleOptions,
            RequireFullScreen = createDto.RequireFullScreen,
            DisableCopyPaste = createDto.DisableCopyPaste
        };

        await _examRepository.AddAsync(exam);

        var createdExam = await _examRepository.GetExamWithPaperAsync(exam.Id);
        return ApiResponse<ExamDto>.SuccessResponse(MapToExamDto(createdExam!), "考试创建成功");
    }

    public async Task<ApiResponse<ExamAttemptDto>> StartExamAsync(Guid examId, Guid userId)
    {
        // 检查是否可以参加考试
        var canTake = await _examRepository.CanUserTakeExamAsync(examId, userId);
        if (!canTake)
        {
            return ApiResponse<ExamAttemptDto>.ErrorResponse("不能参加此考试(考试未开始、已结束或已达到最大尝试次数)");
        }

        var exam = await _examRepository.GetExamWithPaperAsync(examId);
        if (exam == null)
        {
            return ApiResponse<ExamAttemptDto>.ErrorResponse("考试不存在");
        }

        // 检查是否有未完成的答题记录
        var existingAttempt = await _context.ExamAttempts
            .FirstOrDefaultAsync(ea => ea.ExamId == examId &&
                                      ea.UserId == userId &&
                                      !ea.IsSubmitted);

        if (existingAttempt != null)
        {
            return ApiResponse<ExamAttemptDto>.SuccessResponse(
                MapToExamAttemptDto(existingAttempt),
                "继续上次未完成的考试");
        }

        // 创建新的答题记录
        var attemptNumber = await _examRepository.GetUserAttemptCountAsync(examId, userId) + 1;

        var examAttempt = new ExamAttempt
        {
            ExamId = examId,
            UserId = userId,
            AttemptNumber = attemptNumber,
            StartTime = DateTime.UtcNow,
            IsSubmitted = false,
            IsGraded = false
        };

        await _context.ExamAttempts.AddAsync(examAttempt);
        await _context.SaveChangesAsync();

        // 为每道题创建答题记录
        var questions = exam.Paper.PaperQuestions.Select(pq => new Answer
        {
            ExamAttemptId = examAttempt.Id,
            QuestionId = pq.QuestionId
        }).ToList();

        await _context.Answers.AddRangeAsync(questions);
        await _context.SaveChangesAsync();

        return ApiResponse<ExamAttemptDto>.SuccessResponse(
            MapToExamAttemptDto(examAttempt),
            "考试开始");
    }

    public async Task<ApiResponse<bool>> SubmitAnswerAsync(SubmitAnswerDto submitDto, Guid userId)
    {
        var examAttempt = await _context.ExamAttempts
            .Include(ea => ea.Exam)
            .FirstOrDefaultAsync(ea => ea.Id == submitDto.ExamAttemptId);

        if (examAttempt == null)
        {
            return ApiResponse<bool>.ErrorResponse("答题记录不存在");
        }

        if (examAttempt.UserId != userId)
        {
            return ApiResponse<bool>.ErrorResponse("无权限操作此答题记录");
        }

        if (examAttempt.IsSubmitted)
        {
            return ApiResponse<bool>.ErrorResponse("考试已提交,不能再答题");
        }

        // 检查考试时间是否已过
        if (examAttempt.StartTime.HasValue)
        {
            var elapsed = DateTime.UtcNow - examAttempt.StartTime.Value;
            if (elapsed.TotalMinutes > examAttempt.Exam.Duration)
            {
                return ApiResponse<bool>.ErrorResponse("考试时间已结束");
            }
        }

        // 保存答案
        var answer = await _context.Answers
            .FirstOrDefaultAsync(a => a.ExamAttemptId == submitDto.ExamAttemptId &&
                                     a.QuestionId == submitDto.QuestionId);

        if (answer == null)
        {
            return ApiResponse<bool>.ErrorResponse("答题记录不存在");
        }

        answer.UserAnswer = submitDto.UserAnswer;
        answer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "答案已保存");
    }

    public async Task<ApiResponse<ExamAttemptDto>> SubmitExamAsync(Guid examAttemptId, Guid userId)
    {
        var examAttempt = await _context.ExamAttempts
            .Include(ea => ea.Exam)
                .ThenInclude(e => e.Paper)
                    .ThenInclude(p => p.PaperQuestions)
                        .ThenInclude(pq => pq.Question)
            .Include(ea => ea.Answers)
                .ThenInclude(a => a.Question)
            .FirstOrDefaultAsync(ea => ea.Id == examAttemptId);

        if (examAttempt == null)
        {
            return ApiResponse<ExamAttemptDto>.ErrorResponse("答题记录不存在");
        }

        if (examAttempt.UserId != userId)
        {
            return ApiResponse<ExamAttemptDto>.ErrorResponse("无权限操作此答题记录");
        }

        if (examAttempt.IsSubmitted)
        {
            return ApiResponse<ExamAttemptDto>.ErrorResponse("考试已提交");
        }

        // 标记为已提交
        examAttempt.IsSubmitted = true;
        examAttempt.SubmitTime = DateTime.UtcNow;

        // 自动批改客观题
        int totalScore = 0;
        bool hasSubjectiveQuestions = false;

        foreach (var answer in examAttempt.Answers)
        {
            var question = answer.Question;
            var paperQuestion = examAttempt.Exam.Paper.PaperQuestions
                .FirstOrDefault(pq => pq.QuestionId == question.Id);

            if (paperQuestion == null) continue;

            // 自动批改客观题
            if (question.Type == QuestionType.SingleChoice ||
                question.Type == QuestionType.MultipleChoice ||
                question.Type == QuestionType.TrueFalse)
            {
                if (string.IsNullOrEmpty(answer.UserAnswer))
                {
                    answer.Score = 0;
                    answer.IsCorrect = false;
                }
                else
                {
                    var userAnswer = answer.UserAnswer.Trim().ToUpper();
                    var correctAnswer = question.CorrectAnswer.Trim().ToUpper();

                    // 对于多选题,需要排序后比较
                    if (question.Type == QuestionType.MultipleChoice)
                    {
                        var userAnswers = userAnswer.Split(',').Select(a => a.Trim()).OrderBy(a => a);
                        var correctAnswers = correctAnswer.Split(',').Select(a => a.Trim()).OrderBy(a => a);
                        var isCorrect = userAnswers.SequenceEqual(correctAnswers);

                        answer.IsCorrect = isCorrect;
                        answer.Score = isCorrect ? paperQuestion.Score : 0;
                    }
                    else
                    {
                        var isCorrect = userAnswer == correctAnswer;
                        answer.IsCorrect = isCorrect;
                        answer.Score = isCorrect ? paperQuestion.Score : 0;
                    }
                }

                totalScore += answer.Score ?? 0;
            }
            else
            {
                // 主观题需要手动批改
                hasSubjectiveQuestions = true;
            }
        }

        // 如果全是客观题,则标记为已批改
        if (!hasSubjectiveQuestions)
        {
            examAttempt.IsGraded = true;
            examAttempt.TotalScore = totalScore;
        }

        await _context.SaveChangesAsync();

        return ApiResponse<ExamAttemptDto>.SuccessResponse(
            MapToExamAttemptDto(examAttempt),
            hasSubjectiveQuestions ? "考试已提交,等待教师批改主观题" : "考试已提交并自动批改完成");
    }

    public async Task<ApiResponse<List<ExamAttemptDto>>> GetUserExamAttemptsAsync(Guid userId)
    {
        var attempts = await _context.ExamAttempts
            .Include(ea => ea.Exam)
            .Where(ea => ea.UserId == userId)
            .OrderByDescending(ea => ea.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<ExamAttemptDto>>.SuccessResponse(
            attempts.Select(MapToExamAttemptDto).ToList());
    }

    private ExamDto MapToExamDto(Exam exam)
    {
        return new ExamDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Description = exam.Description,
            PaperId = exam.PaperId,
            Status = exam.Status,
            StartTime = exam.StartTime,
            EndTime = exam.EndTime,
            Duration = exam.Duration,
            MaxAttempts = exam.MaxAttempts,
            CreatedAt = exam.CreatedAt
        };
    }

    private ExamAttemptDto MapToExamAttemptDto(ExamAttempt examAttempt)
    {
        return new ExamAttemptDto
        {
            Id = examAttempt.Id,
            ExamId = examAttempt.ExamId,
            ExamTitle = examAttempt.Exam?.Title ?? "",
            AttemptNumber = examAttempt.AttemptNumber,
            StartTime = examAttempt.StartTime,
            SubmitTime = examAttempt.SubmitTime,
            TotalScore = examAttempt.TotalScore,
            IsSubmitted = examAttempt.IsSubmitted,
            IsGraded = examAttempt.IsGraded
        };
    }
}
