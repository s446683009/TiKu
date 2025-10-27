using Microsoft.EntityFrameworkCore;
using QuestionBank.Application.DTOs;
using QuestionBank.Domain.Entities;
using QuestionBank.Infrastructure.Data;
using QuestionBank.Infrastructure.Repositories;

namespace QuestionBank.Application.Services;

public class LearningService : ILearningService
{
    private readonly ApplicationDbContext _context;
    private readonly IQuestionRepository _questionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LearningService(
        ApplicationDbContext context,
        IQuestionRepository questionRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _questionRepository = questionRepository;
        _unitOfWork = unitOfWork;
    }

    #region 错题本

    public async Task<ApiResponse<WrongQuestionDto>> AddWrongQuestionAsync(Guid userId, AddWrongQuestionDto dto)
    {
        // 验证题目是否存在
        var question = await _questionRepository.GetByIdAsync(dto.QuestionId);
        if (question == null)
        {
            return ApiResponse<WrongQuestionDto>.ErrorResponse("题目不存在");
        }

        // 检查是否已存在
        var existing = await _context.WrongQuestions
            .FirstOrDefaultAsync(w => w.UserId == userId && w.QuestionId == dto.QuestionId);

        if (existing != null)
        {
            // 已存在，更新错误次数和时间
            if (existing.IsRemoved)
            {
                existing.IsRemoved = false;
            }
            existing.WrongCount++;
            existing.LastWrongAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            var existingDto = await MapToWrongQuestionDto(existing);
            return ApiResponse<WrongQuestionDto>.SuccessResponse(existingDto, "错题已更新");
        }

        // 创建新的错题记录
        var wrongQuestion = new WrongQuestion
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            QuestionId = dto.QuestionId,
            WrongCount = 1,
            LastWrongAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WrongQuestions.Add(wrongQuestion);
        await _unitOfWork.SaveChangesAsync();

        var resultDto = await MapToWrongQuestionDto(wrongQuestion);
        return ApiResponse<WrongQuestionDto>.SuccessResponse(resultDto, "已添加到错题本");
    }

    public async Task<ApiResponse<PagedResponse<WrongQuestionDto>>> GetWrongQuestionsAsync(
        Guid userId, int pageNumber = 1, int pageSize = 20)
    {
        var query = _context.WrongQuestions
            .Include(w => w.Question)
                .ThenInclude(q => q.QuestionKnowledgePoints)
                    .ThenInclude(qkp => qkp.KnowledgePoint)
            .Where(w => w.UserId == userId && !w.IsRemoved)
            .OrderByDescending(w => w.LastWrongAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = new List<WrongQuestionDto>();
        foreach (var item in items)
        {
            dtos.Add(await MapToWrongQuestionDto(item));
        }

        var pagedResponse = new PagedResponse<WrongQuestionDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return ApiResponse<PagedResponse<WrongQuestionDto>>.SuccessResponse(pagedResponse);
    }

    public async Task<ApiResponse<bool>> RemoveWrongQuestionAsync(Guid userId, Guid questionId)
    {
        var wrongQuestion = await _context.WrongQuestions
            .FirstOrDefaultAsync(w => w.UserId == userId && w.QuestionId == questionId);

        if (wrongQuestion == null)
        {
            return ApiResponse<bool>.ErrorResponse("错题记录不存在");
        }

        wrongQuestion.IsRemoved = true;
        wrongQuestion.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "已从错题本移除");
    }

    public async Task<ApiResponse<List<WrongQuestionDto>>> GetWrongQuestionsByKnowledgePointAsync(
        Guid userId, Guid knowledgePointId)
    {
        var wrongQuestions = await _context.WrongQuestions
            .Include(w => w.Question)
                .ThenInclude(q => q.QuestionKnowledgePoints)
                    .ThenInclude(qkp => qkp.KnowledgePoint)
            .Where(w => w.UserId == userId && !w.IsRemoved &&
                   w.Question.QuestionKnowledgePoints.Any(qkp => qkp.KnowledgePointId == knowledgePointId))
            .OrderByDescending(w => w.LastWrongAt)
            .ToListAsync();

        var dtos = new List<WrongQuestionDto>();
        foreach (var item in wrongQuestions)
        {
            dtos.Add(await MapToWrongQuestionDto(item));
        }

        return ApiResponse<List<WrongQuestionDto>>.SuccessResponse(dtos);
    }

    #endregion

    #region 收藏

    public async Task<ApiResponse<FavoriteQuestionDto>> AddFavoriteAsync(Guid userId, AddFavoriteDto dto)
    {
        // 验证题目是否存在
        var question = await _questionRepository.GetByIdAsync(dto.QuestionId);
        if (question == null)
        {
            return ApiResponse<FavoriteQuestionDto>.ErrorResponse("题目不存在");
        }

        // 检查是否已收藏
        var existing = await _context.FavoriteQuestions
            .FirstOrDefaultAsync(f => f.UserId == userId && f.QuestionId == dto.QuestionId);

        if (existing != null)
        {
            return ApiResponse<FavoriteQuestionDto>.ErrorResponse("已收藏该题目");
        }

        // 创建收藏记录
        var favorite = new FavoriteQuestion
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            QuestionId = dto.QuestionId,
            Note = dto.Note,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.FavoriteQuestions.Add(favorite);
        await _unitOfWork.SaveChangesAsync();

        var resultDto = await MapToFavoriteQuestionDto(favorite);
        return ApiResponse<FavoriteQuestionDto>.SuccessResponse(resultDto, "收藏成功");
    }

    public async Task<ApiResponse<PagedResponse<FavoriteQuestionDto>>> GetFavoritesAsync(
        Guid userId, int pageNumber = 1, int pageSize = 20)
    {
        var query = _context.FavoriteQuestions
            .Include(f => f.Question)
                .ThenInclude(q => q.QuestionKnowledgePoints)
                    .ThenInclude(qkp => qkp.KnowledgePoint)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = new List<FavoriteQuestionDto>();
        foreach (var item in items)
        {
            dtos.Add(await MapToFavoriteQuestionDto(item));
        }

        var pagedResponse = new PagedResponse<FavoriteQuestionDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return ApiResponse<PagedResponse<FavoriteQuestionDto>>.SuccessResponse(pagedResponse);
    }

    public async Task<ApiResponse<bool>> RemoveFavoriteAsync(Guid userId, Guid questionId)
    {
        var favorite = await _context.FavoriteQuestions
            .FirstOrDefaultAsync(f => f.UserId == userId && f.QuestionId == questionId);

        if (favorite == null)
        {
            return ApiResponse<bool>.ErrorResponse("收藏记录不存在");
        }

        _context.FavoriteQuestions.Remove(favorite);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "取消收藏成功");
    }

    public async Task<ApiResponse<FavoriteQuestionDto>> UpdateFavoriteNoteAsync(
        Guid userId, Guid questionId, UpdateFavoriteDto dto)
    {
        var favorite = await _context.FavoriteQuestions
            .Include(f => f.Question)
                .ThenInclude(q => q.QuestionKnowledgePoints)
                    .ThenInclude(qkp => qkp.KnowledgePoint)
            .FirstOrDefaultAsync(f => f.UserId == userId && f.QuestionId == questionId);

        if (favorite == null)
        {
            return ApiResponse<FavoriteQuestionDto>.ErrorResponse("收藏记录不存在");
        }

        favorite.Note = dto.Note;
        favorite.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        var resultDto = await MapToFavoriteQuestionDto(favorite);
        return ApiResponse<FavoriteQuestionDto>.SuccessResponse(resultDto, "备注更新成功");
    }

    #endregion

    #region 笔记

    public async Task<ApiResponse<QuestionNoteDto>> CreateNoteAsync(Guid userId, CreateQuestionNoteDto dto)
    {
        // 验证题目是否存在
        var question = await _questionRepository.GetByIdAsync(dto.QuestionId);
        if (question == null)
        {
            return ApiResponse<QuestionNoteDto>.ErrorResponse("题目不存在");
        }

        // 检查是否已有笔记
        var existing = await _context.QuestionNotes
            .FirstOrDefaultAsync(n => n.UserId == userId && n.QuestionId == dto.QuestionId);

        if (existing != null)
        {
            return ApiResponse<QuestionNoteDto>.ErrorResponse("该题目已有笔记，请使用更新功能");
        }

        // 创建笔记
        var note = new QuestionNote
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            QuestionId = dto.QuestionId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.QuestionNotes.Add(note);
        await _unitOfWork.SaveChangesAsync();

        var resultDto = await MapToQuestionNoteDto(note, true);
        return ApiResponse<QuestionNoteDto>.SuccessResponse(resultDto, "笔记创建成功");
    }

    public async Task<ApiResponse<QuestionNoteDto>> GetNoteByQuestionIdAsync(Guid userId, Guid questionId)
    {
        var note = await _context.QuestionNotes
            .Include(n => n.Question)
                .ThenInclude(q => q.QuestionKnowledgePoints)
                    .ThenInclude(qkp => qkp.KnowledgePoint)
            .FirstOrDefaultAsync(n => n.UserId == userId && n.QuestionId == questionId);

        if (note == null)
        {
            return ApiResponse<QuestionNoteDto>.ErrorResponse("笔记不存在");
        }

        var dto = await MapToQuestionNoteDto(note, true);
        return ApiResponse<QuestionNoteDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<PagedResponse<QuestionNoteDto>>> GetUserNotesAsync(
        Guid userId, int pageNumber = 1, int pageSize = 20)
    {
        var query = _context.QuestionNotes
            .Include(n => n.Question)
                .ThenInclude(q => q.QuestionKnowledgePoints)
                    .ThenInclude(qkp => qkp.KnowledgePoint)
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.UpdatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = new List<QuestionNoteDto>();
        foreach (var item in items)
        {
            dtos.Add(await MapToQuestionNoteDto(item, true));
        }

        var pagedResponse = new PagedResponse<QuestionNoteDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return ApiResponse<PagedResponse<QuestionNoteDto>>.SuccessResponse(pagedResponse);
    }

    public async Task<ApiResponse<QuestionNoteDto>> UpdateNoteAsync(
        Guid userId, Guid noteId, UpdateQuestionNoteDto dto)
    {
        var note = await _context.QuestionNotes
            .Include(n => n.Question)
                .ThenInclude(q => q.QuestionKnowledgePoints)
                    .ThenInclude(qkp => qkp.KnowledgePoint)
            .FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);

        if (note == null)
        {
            return ApiResponse<QuestionNoteDto>.ErrorResponse("笔记不存在");
        }

        note.Content = dto.Content;
        note.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        var resultDto = await MapToQuestionNoteDto(note, true);
        return ApiResponse<QuestionNoteDto>.SuccessResponse(resultDto, "笔记更新成功");
    }

    public async Task<ApiResponse<bool>> DeleteNoteAsync(Guid userId, Guid noteId)
    {
        var note = await _context.QuestionNotes
            .FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);

        if (note == null)
        {
            return ApiResponse<bool>.ErrorResponse("笔记不存在");
        }

        _context.QuestionNotes.Remove(note);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "笔记删除成功");
    }

    #endregion

    #region 私有辅助方法

    private async Task<WrongQuestionDto> MapToWrongQuestionDto(WrongQuestion wrongQuestion)
    {
        // 确保Question已加载
        if (!_context.Entry(wrongQuestion).Reference(w => w.Question).IsLoaded)
        {
            await _context.Entry(wrongQuestion).Reference(w => w.Question).LoadAsync();
        }

        return new WrongQuestionDto
        {
            Id = wrongQuestion.Id,
            QuestionId = wrongQuestion.QuestionId,
            WrongCount = wrongQuestion.WrongCount,
            LastWrongAt = wrongQuestion.LastWrongAt,
            Question = await MapToQuestionDto(wrongQuestion.Question)
        };
    }

    private async Task<FavoriteQuestionDto> MapToFavoriteQuestionDto(FavoriteQuestion favorite)
    {
        // 确保Question已加载
        if (!_context.Entry(favorite).Reference(f => f.Question).IsLoaded)
        {
            await _context.Entry(favorite).Reference(f => f.Question).LoadAsync();
        }

        return new FavoriteQuestionDto
        {
            Id = favorite.Id,
            QuestionId = favorite.QuestionId,
            Note = favorite.Note,
            CreatedAt = favorite.CreatedAt,
            Question = await MapToQuestionDto(favorite.Question)
        };
    }

    private async Task<QuestionNoteDto> MapToQuestionNoteDto(QuestionNote note, bool includeQuestion = false)
    {
        var dto = new QuestionNoteDto
        {
            Id = note.Id,
            QuestionId = note.QuestionId,
            Content = note.Content,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt ?? note.CreatedAt
        };

        if (includeQuestion)
        {
            if (!_context.Entry(note).Reference(n => n.Question).IsLoaded)
            {
                await _context.Entry(note).Reference(n => n.Question).LoadAsync();
            }
            dto.Question = await MapToQuestionDto(note.Question);
        }

        return dto;
    }

    private async Task<QuestionDto> MapToQuestionDto(Question question)
    {
        // 确保知识点已加载
        if (!_context.Entry(question).Collection(q => q.QuestionKnowledgePoints).IsLoaded)
        {
            await _context.Entry(question).Collection(q => q.QuestionKnowledgePoints).LoadAsync();
            foreach (var qkp in question.QuestionKnowledgePoints)
            {
                await _context.Entry(qkp).Reference(x => x.KnowledgePoint).LoadAsync();
            }
        }

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
                .Select(qkp => new KnowledgePointDto
                {
                    Id = qkp.KnowledgePoint.Id,
                    Name = qkp.KnowledgePoint.Name,
                    Description = qkp.KnowledgePoint.Description,
                    Level = qkp.KnowledgePoint.Level
                }).ToList(),
            CreatedAt = question.CreatedAt
        };
    }

    #endregion
}
