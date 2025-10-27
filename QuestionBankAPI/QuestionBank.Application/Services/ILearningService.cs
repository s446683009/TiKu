using QuestionBank.Application.DTOs;

namespace QuestionBank.Application.Services;

public interface ILearningService
{
    // 错题本相关
    /// <summary>
    /// 添加题目到错题本
    /// </summary>
    Task<ApiResponse<WrongQuestionDto>> AddWrongQuestionAsync(Guid userId, AddWrongQuestionDto dto);

    /// <summary>
    /// 获取用户的错题本列表
    /// </summary>
    Task<ApiResponse<PagedResponse<WrongQuestionDto>>> GetWrongQuestionsAsync(Guid userId, int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// 从错题本移除题目
    /// </summary>
    Task<ApiResponse<bool>> RemoveWrongQuestionAsync(Guid userId, Guid questionId);

    /// <summary>
    /// 按知识点获取错题
    /// </summary>
    Task<ApiResponse<List<WrongQuestionDto>>> GetWrongQuestionsByKnowledgePointAsync(Guid userId, Guid knowledgePointId);

    // 收藏相关
    /// <summary>
    /// 收藏题目
    /// </summary>
    Task<ApiResponse<FavoriteQuestionDto>> AddFavoriteAsync(Guid userId, AddFavoriteDto dto);

    /// <summary>
    /// 获取用户的收藏列表
    /// </summary>
    Task<ApiResponse<PagedResponse<FavoriteQuestionDto>>> GetFavoritesAsync(Guid userId, int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// 取消收藏
    /// </summary>
    Task<ApiResponse<bool>> RemoveFavoriteAsync(Guid userId, Guid questionId);

    /// <summary>
    /// 更新收藏备注
    /// </summary>
    Task<ApiResponse<FavoriteQuestionDto>> UpdateFavoriteNoteAsync(Guid userId, Guid questionId, UpdateFavoriteDto dto);

    // 笔记相关
    /// <summary>
    /// 创建题目笔记
    /// </summary>
    Task<ApiResponse<QuestionNoteDto>> CreateNoteAsync(Guid userId, CreateQuestionNoteDto dto);

    /// <summary>
    /// 获取题目的笔记
    /// </summary>
    Task<ApiResponse<QuestionNoteDto>> GetNoteByQuestionIdAsync(Guid userId, Guid questionId);

    /// <summary>
    /// 获取用户的所有笔记
    /// </summary>
    Task<ApiResponse<PagedResponse<QuestionNoteDto>>> GetUserNotesAsync(Guid userId, int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// 更新笔记
    /// </summary>
    Task<ApiResponse<QuestionNoteDto>> UpdateNoteAsync(Guid userId, Guid noteId, UpdateQuestionNoteDto dto);

    /// <summary>
    /// 删除笔记
    /// </summary>
    Task<ApiResponse<bool>> DeleteNoteAsync(Guid userId, Guid noteId);
}
