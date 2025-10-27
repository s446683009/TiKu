using QuestionBank.Application.DTOs;

namespace QuestionBank.Application.Services;

public interface IPaperService
{
    /// <summary>
    /// 获取试卷详情（包含题目列表）
    /// </summary>
    Task<ApiResponse<PaperDto>> GetPaperByIdAsync(Guid id);

    /// <summary>
    /// 获取所有试卷列表
    /// </summary>
    Task<ApiResponse<PagedResponse<PaperDto>>> GetAllPapersAsync(int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// 创建试卷（手动组卷）
    /// </summary>
    Task<ApiResponse<PaperDto>> CreatePaperAsync(CreatePaperDto createDto, Guid creatorId);

    /// <summary>
    /// 更新试卷
    /// </summary>
    Task<ApiResponse<PaperDto>> UpdatePaperAsync(Guid id, CreatePaperDto updateDto);

    /// <summary>
    /// 删除试卷
    /// </summary>
    Task<ApiResponse<bool>> DeletePaperAsync(Guid id);

    /// <summary>
    /// 获取试卷的完整信息（含题目详情）
    /// </summary>
    Task<ApiResponse<PaperDetailDto>> GetPaperDetailAsync(Guid id);
}
