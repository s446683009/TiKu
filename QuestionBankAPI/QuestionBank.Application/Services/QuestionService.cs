using Microsoft.EntityFrameworkCore;
using QuestionBank.Application.DTOs;
using QuestionBank.Domain.Entities;
using QuestionBank.Infrastructure.Data;
using QuestionBank.Infrastructure.Repositories;

namespace QuestionBank.Application.Services;

public class QuestionService : IQuestionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public QuestionService(IUnitOfWork unitOfWork, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<ApiResponse<QuestionDto>> GetQuestionByIdAsync(Guid id)
    {
        var question = await _context.Questions
            .Include(q => q.QuestionKnowledgePoints)
                .ThenInclude(qk => qk.KnowledgePoint)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
        {
            return ApiResponse<QuestionDto>.ErrorResponse("题目不存在");
        }

        return ApiResponse<QuestionDto>.SuccessResponse(MapToQuestionDto(question));
    }

    public async Task<ApiResponse<PagedResponse<QuestionDto>>> SearchQuestionsAsync(QuestionSearchDto searchDto)
    {
        var query = _context.Questions
            .Include(q => q.QuestionKnowledgePoints)
                .ThenInclude(qk => qk.KnowledgePoint)
            .AsQueryable();

        // 关键字搜索
        if (!string.IsNullOrWhiteSpace(searchDto.Keyword))
        {
            query = query.Where(q => q.Content.Contains(searchDto.Keyword) ||
                                    (q.Explanation != null && q.Explanation.Contains(searchDto.Keyword)));
        }

        // 题型筛选
        if (searchDto.Type.HasValue)
        {
            query = query.Where(q => q.Type == searchDto.Type.Value);
        }

        // 难度筛选
        if (searchDto.Difficulty.HasValue)
        {
            query = query.Where(q => q.Difficulty == searchDto.Difficulty.Value);
        }

        // 知识点筛选
        if (searchDto.KnowledgePointId.HasValue)
        {
            query = query.Where(q => q.QuestionKnowledgePoints
                .Any(qk => qk.KnowledgePointId == searchDto.KnowledgePointId.Value));
        }

        // 章节筛选
        if (!string.IsNullOrWhiteSpace(searchDto.Chapter))
        {
            query = query.Where(q => q.Chapter == searchDto.Chapter);
        }

        // 状态筛选
        if (searchDto.Status.HasValue)
        {
            query = query.Where(q => q.Status == searchDto.Status.Value);
        }

        var totalCount = await query.CountAsync();

        var questions = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .ToListAsync();

        var pagedResponse = new PagedResponse<QuestionDto>
        {
            Items = questions.Select(MapToQuestionDto).ToList(),
            TotalCount = totalCount,
            PageNumber = searchDto.PageNumber,
            PageSize = searchDto.PageSize
        };

        return ApiResponse<PagedResponse<QuestionDto>>.SuccessResponse(pagedResponse);
    }

    public async Task<ApiResponse<QuestionDto>> CreateQuestionAsync(CreateQuestionDto createDto, Guid creatorId)
    {
        var question = new Question
        {
            Type = createDto.Type,
            Content = createDto.Content,
            Options = createDto.Options,
            CorrectAnswer = createDto.CorrectAnswer,
            Explanation = createDto.Explanation,
            Difficulty = createDto.Difficulty,
            Score = createDto.Score,
            Chapter = createDto.Chapter,
            CreatorId = creatorId,
            Status = Domain.Enums.QuestionStatus.Enabled
        };

        await _unitOfWork.Repository<Question>().AddAsync(question);

        // 关联知识点
        if (createDto.KnowledgePointIds.Any())
        {
            var knowledgePointLinks = createDto.KnowledgePointIds.Select(kpId => new QuestionKnowledgePoint
            {
                QuestionId = question.Id,
                KnowledgePointId = kpId
            }).ToList();

            await _context.QuestionKnowledgePoints.AddRangeAsync(knowledgePointLinks);
            await _context.SaveChangesAsync();
        }

        // 重新查询以包含关联数据
        var createdQuestion = await _context.Questions
            .Include(q => q.QuestionKnowledgePoints)
                .ThenInclude(qk => qk.KnowledgePoint)
            .FirstOrDefaultAsync(q => q.Id == question.Id);

        return ApiResponse<QuestionDto>.SuccessResponse(MapToQuestionDto(createdQuestion!), "题目创建成功");
    }

    public async Task<ApiResponse<QuestionDto>> UpdateQuestionAsync(UpdateQuestionDto updateDto)
    {
        var question = await _context.Questions
            .Include(q => q.QuestionKnowledgePoints)
            .FirstOrDefaultAsync(q => q.Id == updateDto.Id);

        if (question == null)
        {
            return ApiResponse<QuestionDto>.ErrorResponse("题目不存在");
        }

        question.Type = updateDto.Type;
        question.Content = updateDto.Content;
        question.Options = updateDto.Options;
        question.CorrectAnswer = updateDto.CorrectAnswer;
        question.Explanation = updateDto.Explanation;
        question.Difficulty = updateDto.Difficulty;
        question.Score = updateDto.Score;
        question.Chapter = updateDto.Chapter;
        question.Status = updateDto.Status;
        question.UpdatedAt = DateTime.UtcNow;

        // 更新知识点关联
        _context.QuestionKnowledgePoints.RemoveRange(question.QuestionKnowledgePoints);

        if (updateDto.KnowledgePointIds.Any())
        {
            var knowledgePointLinks = updateDto.KnowledgePointIds.Select(kpId => new QuestionKnowledgePoint
            {
                QuestionId = question.Id,
                KnowledgePointId = kpId
            }).ToList();

            await _context.QuestionKnowledgePoints.AddRangeAsync(knowledgePointLinks);
        }

        await _context.SaveChangesAsync();

        // 重新查询
        var updatedQuestion = await _context.Questions
            .Include(q => q.QuestionKnowledgePoints)
                .ThenInclude(qk => qk.KnowledgePoint)
            .FirstOrDefaultAsync(q => q.Id == question.Id);

        return ApiResponse<QuestionDto>.SuccessResponse(MapToQuestionDto(updatedQuestion!), "题目更新成功");
    }

    public async Task<ApiResponse<bool>> DeleteQuestionAsync(Guid id)
    {
        var question = await _unitOfWork.Repository<Question>().GetByIdAsync(id);
        if (question == null)
        {
            return ApiResponse<bool>.ErrorResponse("题目不存在");
        }

        // 软删除
        question.IsDeleted = true;
        question.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<Question>().UpdateAsync(question);

        return ApiResponse<bool>.SuccessResponse(true, "题目删除成功");
    }

    public async Task<ApiResponse<List<QuestionDto>>> GetQuestionsByKnowledgePointAsync(Guid knowledgePointId)
    {
        var questions = await _context.Questions
            .Include(q => q.QuestionKnowledgePoints)
                .ThenInclude(qk => qk.KnowledgePoint)
            .Where(q => q.QuestionKnowledgePoints.Any(qk => qk.KnowledgePointId == knowledgePointId))
            .ToListAsync();

        return ApiResponse<List<QuestionDto>>.SuccessResponse(
            questions.Select(MapToQuestionDto).ToList());
    }

    private QuestionDto MapToQuestionDto(Question question)
    {
        return new QuestionDto
        {
            Id = question.Id,
            Type = question.Type,
            Content = question.Content,
            Options = question.Options,
            CorrectAnswer = question.CorrectAnswer,
            Explanation = question.Explanation,
            Difficulty = question.Difficulty,
            Score = question.Score,
            Chapter = question.Chapter,
            Status = question.Status,
            KnowledgePoints = question.QuestionKnowledgePoints
                .Select(qk => new KnowledgePointDto
                {
                    Id = qk.KnowledgePoint.Id,
                    Name = qk.KnowledgePoint.Name,
                    Description = qk.KnowledgePoint.Description,
                    ParentId = qk.KnowledgePoint.ParentId,
                    Level = qk.KnowledgePoint.Level
                }).ToList(),
            CreatedAt = question.CreatedAt
        };
    }
}
