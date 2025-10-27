using QuestionBank.Application.DTOs;

namespace QuestionBank.Application.Services;

public interface IKnowledgePointService
{
    Task<ApiResponse<KnowledgePointDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<List<KnowledgePointDto>>> GetAllAsync();
    Task<ApiResponse<List<KnowledgePointDto>>> GetRootKnowledgePointsAsync();
    Task<ApiResponse<List<KnowledgePointDto>>> GetChildrenAsync(Guid parentId);
    Task<ApiResponse<KnowledgePointDto>> GetKnowledgeTreeAsync(Guid id);
    Task<ApiResponse<KnowledgePointDto>> CreateAsync(CreateKnowledgePointDto createDto);
    Task<ApiResponse<KnowledgePointDto>> UpdateAsync(Guid id, CreateKnowledgePointDto updateDto);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}
