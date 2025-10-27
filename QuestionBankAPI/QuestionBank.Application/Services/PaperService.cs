using Microsoft.EntityFrameworkCore;
using QuestionBank.Application.DTOs;
using QuestionBank.Domain.Entities;
using QuestionBank.Infrastructure.Data;
using QuestionBank.Infrastructure.Repositories;

namespace QuestionBank.Application.Services;

public class PaperService : IPaperService
{
    private readonly IPaperRepository _paperRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public PaperService(
        IPaperRepository paperRepository,
        IQuestionRepository questionRepository,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork)
    {
        _paperRepository = paperRepository;
        _questionRepository = questionRepository;
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PaperDto>> GetPaperByIdAsync(Guid id)
    {
        var paper = await _paperRepository.GetByIdAsync(id);
        if (paper == null)
        {
            return ApiResponse<PaperDto>.ErrorResponse("试卷不存在");
        }

        var paperDto = await MapToPaperDto(paper);
        return ApiResponse<PaperDto>.SuccessResponse(paperDto);
    }

    public async Task<ApiResponse<PagedResponse<PaperDto>>> GetAllPapersAsync(int pageNumber = 1, int pageSize = 20)
    {
        var query = _context.Papers
            .Include(p => p.PaperQuestions)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var papers = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var paperDtos = new List<PaperDto>();
        foreach (var paper in papers)
        {
            paperDtos.Add(await MapToPaperDto(paper));
        }

        var pagedResponse = new PagedResponse<PaperDto>
        {
            Items = paperDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return ApiResponse<PagedResponse<PaperDto>>.SuccessResponse(pagedResponse);
    }

    public async Task<ApiResponse<PaperDto>> CreatePaperAsync(CreatePaperDto createDto, Guid creatorId)
    {
        // 验证题目是否都存在
        var questionIds = createDto.Questions.Select(q => q.QuestionId).ToList();
        var questions = await _questionRepository.GetQuestionsByIdsAsync(questionIds);

        if (questions.Count != questionIds.Count)
        {
            return ApiResponse<PaperDto>.ErrorResponse("部分题目不存在");
        }

        // 计算总分
        var totalScore = createDto.Questions.Sum(q => q.Score);

        var paper = new Paper
        {
            Id = Guid.NewGuid(),
            Title = createDto.Title,
            Description = createDto.Description,
            TotalScore = totalScore,
            Duration = createDto.Duration,
            CreatorId = creatorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 添加试卷题目关联
        foreach (var questionDto in createDto.Questions)
        {
            paper.PaperQuestions.Add(new PaperQuestion
            {
                PaperId = paper.Id,
                QuestionId = questionDto.QuestionId,
                QuestionOrder = questionDto.QuestionOrder,
                Score = questionDto.Score
            });
        }

        await _paperRepository.AddAsync(paper);
        await _unitOfWork.SaveChangesAsync();

        var createdPaper = await _paperRepository.GetByIdAsync(paper.Id);
        var paperDto = await MapToPaperDto(createdPaper!);

        return ApiResponse<PaperDto>.SuccessResponse(paperDto, "试卷创建成功");
    }

    public async Task<ApiResponse<PaperDto>> UpdatePaperAsync(Guid id, CreatePaperDto updateDto)
    {
        var paper = await _context.Papers
            .Include(p => p.PaperQuestions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (paper == null)
        {
            return ApiResponse<PaperDto>.ErrorResponse("试卷不存在");
        }

        // 验证题目是否都存在
        var questionIds = updateDto.Questions.Select(q => q.QuestionId).ToList();
        var questions = await _questionRepository.GetQuestionsByIdsAsync(questionIds);

        if (questions.Count != questionIds.Count)
        {
            return ApiResponse<PaperDto>.ErrorResponse("部分题目不存在");
        }

        // 计算总分
        var totalScore = updateDto.Questions.Sum(q => q.Score);

        // 更新试卷基本信息
        paper.Title = updateDto.Title;
        paper.Description = updateDto.Description;
        paper.TotalScore = totalScore;
        paper.Duration = updateDto.Duration;
        paper.UpdatedAt = DateTime.UtcNow;

        // 删除原有的题目关联
        _context.PaperQuestions.RemoveRange(paper.PaperQuestions);

        // 添加新的题目关联
        foreach (var questionDto in updateDto.Questions)
        {
            paper.PaperQuestions.Add(new PaperQuestion
            {
                PaperId = paper.Id,
                QuestionId = questionDto.QuestionId,
                QuestionOrder = questionDto.QuestionOrder,
                Score = questionDto.Score
            });
        }

        await _unitOfWork.SaveChangesAsync();

        var updatedPaper = await _paperRepository.GetByIdAsync(paper.Id);
        var paperDto = await MapToPaperDto(updatedPaper!);

        return ApiResponse<PaperDto>.SuccessResponse(paperDto, "试卷更新成功");
    }

    public async Task<ApiResponse<bool>> DeletePaperAsync(Guid id)
    {
        var paper = await _paperRepository.GetByIdAsync(id);
        if (paper == null)
        {
            return ApiResponse<bool>.ErrorResponse("试卷不存在");
        }

        // 检查是否有关联的考试
        var hasExams = await _context.Exams.AnyAsync(e => e.PaperId == id);
        if (hasExams)
        {
            return ApiResponse<bool>.ErrorResponse("该试卷已被用于考试，无法删除");
        }

        await _paperRepository.DeleteAsync(paper);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "试卷删除成功");
    }

    public async Task<ApiResponse<PaperDetailDto>> GetPaperDetailAsync(Guid id)
    {
        var paper = await _context.Papers
            .Include(p => p.PaperQuestions)
                .ThenInclude(pq => pq.Question)
                    .ThenInclude(q => q.QuestionKnowledgePoints)
                        .ThenInclude(qkp => qkp.KnowledgePoint)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (paper == null)
        {
            return ApiResponse<PaperDetailDto>.ErrorResponse("试卷不存在");
        }

        var paperDetail = new PaperDetailDto
        {
            Id = paper.Id,
            Title = paper.Title,
            Description = paper.Description,
            TotalScore = paper.TotalScore,
            Duration = paper.Duration,
            CreatedAt = paper.CreatedAt,
            Questions = paper.PaperQuestions
                .OrderBy(pq => pq.QuestionOrder)
                .Select(pq => new PaperQuestionDetailDto
                {
                    QuestionId = pq.QuestionId,
                    QuestionOrder = pq.QuestionOrder,
                    Score = pq.Score,
                    Question = new QuestionDto
                    {
                        Id = pq.Question.Id,
                        Type = pq.Question.Type,
                        Content = pq.Question.Content,
                        Options = pq.Question.Options,
                        CorrectAnswer = pq.Question.CorrectAnswer,
                        Explanation = pq.Question.Explanation,
                        Difficulty = pq.Question.Difficulty,
                        Score = pq.Question.Score,
                        Chapter = pq.Question.Chapter,
                        Status = pq.Question.Status,
                        KnowledgePoints = pq.Question.QuestionKnowledgePoints
                            .Select(qkp => new KnowledgePointDto
                            {
                                Id = qkp.KnowledgePoint.Id,
                                Name = qkp.KnowledgePoint.Name,
                                Description = qkp.KnowledgePoint.Description,
                                Level = qkp.KnowledgePoint.Level
                            }).ToList(),
                        CreatedAt = pq.Question.CreatedAt
                    }
                }).ToList()
        };

        return ApiResponse<PaperDetailDto>.SuccessResponse(paperDetail);
    }

    private async Task<PaperDto> MapToPaperDto(Paper paper)
    {
        // Ensure PaperQuestions is loaded
        if (!_context.Entry(paper).Collection(p => p.PaperQuestions).IsLoaded)
        {
            await _context.Entry(paper).Collection(p => p.PaperQuestions).LoadAsync();
        }

        return new PaperDto
        {
            Id = paper.Id,
            Title = paper.Title,
            Description = paper.Description,
            TotalScore = paper.TotalScore,
            Duration = paper.Duration,
            QuestionCount = paper.PaperQuestions.Count,
            CreatedAt = paper.CreatedAt
        };
    }
}
