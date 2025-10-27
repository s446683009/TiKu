using Microsoft.EntityFrameworkCore;
using QuestionBank.Application.DTOs;
using QuestionBank.Domain.Entities;
using QuestionBank.Infrastructure.Data;
using QuestionBank.Infrastructure.Repositories;

namespace QuestionBank.Application.Services;

public class KnowledgePointService : IKnowledgePointService
{
    private readonly IRepository<KnowledgePoint> _repository;
    private readonly ApplicationDbContext _context;

    public KnowledgePointService(IRepository<KnowledgePoint> repository, ApplicationDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<ApiResponse<KnowledgePointDto>> GetByIdAsync(Guid id)
    {
        var kp = await _context.KnowledgePoints
            .Include(k => k.Children)
            .FirstOrDefaultAsync(k => k.Id == id);

        if (kp == null)
        {
            return ApiResponse<KnowledgePointDto>.ErrorResponse("知识点不存在");
        }

        return ApiResponse<KnowledgePointDto>.SuccessResponse(MapToDto(kp));
    }

    public async Task<ApiResponse<List<KnowledgePointDto>>> GetAllAsync()
    {
        var kps = await _context.KnowledgePoints
            .Include(k => k.Children)
            .OrderBy(k => k.Level)
            .ThenBy(k => k.SortOrder)
            .ToListAsync();

        return ApiResponse<List<KnowledgePointDto>>.SuccessResponse(
            kps.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<List<KnowledgePointDto>>> GetRootKnowledgePointsAsync()
    {
        var roots = await _context.KnowledgePoints
            .Where(k => k.ParentId == null)
            .Include(k => k.Children)
            .OrderBy(k => k.SortOrder)
            .ToListAsync();

        return ApiResponse<List<KnowledgePointDto>>.SuccessResponse(
            roots.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<List<KnowledgePointDto>>> GetChildrenAsync(Guid parentId)
    {
        var children = await _context.KnowledgePoints
            .Where(k => k.ParentId == parentId)
            .Include(k => k.Children)
            .OrderBy(k => k.SortOrder)
            .ToListAsync();

        return ApiResponse<List<KnowledgePointDto>>.SuccessResponse(
            children.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<KnowledgePointDto>> GetKnowledgeTreeAsync(Guid id)
    {
        var kp = await LoadKnowledgeTreeAsync(id);
        if (kp == null)
        {
            return ApiResponse<KnowledgePointDto>.ErrorResponse("知识点不存在");
        }

        return ApiResponse<KnowledgePointDto>.SuccessResponse(MapToDtoWithTree(kp));
    }

    public async Task<ApiResponse<KnowledgePointDto>> CreateAsync(CreateKnowledgePointDto createDto)
    {
        // 验证父知识点(如果有)
        int level = 1;
        if (createDto.ParentId.HasValue)
        {
            var parent = await _repository.GetByIdAsync(createDto.ParentId.Value);
            if (parent == null)
            {
                return ApiResponse<KnowledgePointDto>.ErrorResponse("父知识点不存在");
            }
            level = parent.Level + 1;
        }

        var kp = new KnowledgePoint
        {
            Name = createDto.Name,
            Description = createDto.Description,
            ParentId = createDto.ParentId,
            Level = level,
            SortOrder = await GetNextSortOrderAsync(createDto.ParentId)
        };

        await _repository.AddAsync(kp);

        return ApiResponse<KnowledgePointDto>.SuccessResponse(MapToDto(kp), "知识点创建成功");
    }

    public async Task<ApiResponse<KnowledgePointDto>> UpdateAsync(Guid id, CreateKnowledgePointDto updateDto)
    {
        var kp = await _repository.GetByIdAsync(id);
        if (kp == null)
        {
            return ApiResponse<KnowledgePointDto>.ErrorResponse("知识点不存在");
        }

        // 如果更改了父节点,需要验证
        if (updateDto.ParentId != kp.ParentId)
        {
            // 不能将节点设置为自己的子节点
            if (updateDto.ParentId.HasValue && await IsDescendantAsync(id, updateDto.ParentId.Value))
            {
                return ApiResponse<KnowledgePointDto>.ErrorResponse("不能将节点移动到其子节点下");
            }

            if (updateDto.ParentId.HasValue)
            {
                var parent = await _repository.GetByIdAsync(updateDto.ParentId.Value);
                if (parent == null)
                {
                    return ApiResponse<KnowledgePointDto>.ErrorResponse("父知识点不存在");
                }
                kp.Level = parent.Level + 1;
            }
            else
            {
                kp.Level = 1;
            }
        }

        kp.Name = updateDto.Name;
        kp.Description = updateDto.Description;
        kp.ParentId = updateDto.ParentId;
        kp.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(kp);

        return ApiResponse<KnowledgePointDto>.SuccessResponse(MapToDto(kp), "知识点更新成功");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var kp = await _context.KnowledgePoints
            .Include(k => k.Children)
            .Include(k => k.QuestionKnowledgePoints)
            .FirstOrDefaultAsync(k => k.Id == id);

        if (kp == null)
        {
            return ApiResponse<bool>.ErrorResponse("知识点不存在");
        }

        if (kp.Children.Any())
        {
            return ApiResponse<bool>.ErrorResponse("该知识点下还有子知识点,不能删除");
        }

        if (kp.QuestionKnowledgePoints.Any())
        {
            return ApiResponse<bool>.ErrorResponse("该知识点已关联题目,不能删除");
        }

        kp.IsDeleted = true;
        kp.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(kp);

        return ApiResponse<bool>.SuccessResponse(true, "知识点删除成功");
    }

    private async Task<KnowledgePoint?> LoadKnowledgeTreeAsync(Guid id)
    {
        return await _context.KnowledgePoints
            .Include(k => k.Children)
                .ThenInclude(c => c.Children)
                    .ThenInclude(c => c.Children)
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    private async Task<int> GetNextSortOrderAsync(Guid? parentId)
    {
        var maxOrder = await _context.KnowledgePoints
            .Where(k => k.ParentId == parentId)
            .MaxAsync(k => (int?)k.SortOrder);

        return (maxOrder ?? 0) + 1;
    }

    private async Task<bool> IsDescendantAsync(Guid ancestorId, Guid descendantId)
    {
        var kp = await _repository.GetByIdAsync(descendantId);
        while (kp != null && kp.ParentId.HasValue)
        {
            if (kp.ParentId.Value == ancestorId)
                return true;

            kp = await _repository.GetByIdAsync(kp.ParentId.Value);
        }
        return false;
    }

    private KnowledgePointDto MapToDto(KnowledgePoint kp)
    {
        return new KnowledgePointDto
        {
            Id = kp.Id,
            Name = kp.Name,
            Description = kp.Description,
            ParentId = kp.ParentId,
            Level = kp.Level,
            Children = new List<KnowledgePointDto>()
        };
    }

    private KnowledgePointDto MapToDtoWithTree(KnowledgePoint kp)
    {
        return new KnowledgePointDto
        {
            Id = kp.Id,
            Name = kp.Name,
            Description = kp.Description,
            ParentId = kp.ParentId,
            Level = kp.Level,
            Children = kp.Children.Select(MapToDtoWithTree).ToList()
        };
    }
}
