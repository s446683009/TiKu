using QuestionBank.Application.DTOs;

namespace QuestionBank.Application.Services;

public interface IQuestionService
{
    Task<ApiResponse<QuestionDto>> GetQuestionByIdAsync(Guid id);
    Task<ApiResponse<PagedResponse<QuestionDto>>> SearchQuestionsAsync(QuestionSearchDto searchDto);
    Task<ApiResponse<QuestionDto>> CreateQuestionAsync(CreateQuestionDto createDto, Guid creatorId);
    Task<ApiResponse<QuestionDto>> UpdateQuestionAsync(UpdateQuestionDto updateDto);
    Task<ApiResponse<bool>> DeleteQuestionAsync(Guid id);
    Task<ApiResponse<List<QuestionDto>>> GetQuestionsByKnowledgePointAsync(Guid knowledgePointId);
}
