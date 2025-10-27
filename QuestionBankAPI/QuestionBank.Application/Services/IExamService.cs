using QuestionBank.Application.DTOs;

namespace QuestionBank.Application.Services;

public interface IExamService
{
    Task<ApiResponse<ExamDto>> GetExamByIdAsync(Guid id);
    Task<ApiResponse<List<ExamDto>>> GetAllExamsAsync();
    Task<ApiResponse<ExamDto>> CreateExamAsync(CreateExamDto createDto, Guid creatorId);
    Task<ApiResponse<ExamAttemptDto>> StartExamAsync(Guid examId, Guid userId);
    Task<ApiResponse<bool>> SubmitAnswerAsync(SubmitAnswerDto submitDto, Guid userId);
    Task<ApiResponse<ExamAttemptDto>> SubmitExamAsync(Guid examAttemptId, Guid userId);
    Task<ApiResponse<List<ExamAttemptDto>>> GetUserExamAttemptsAsync(Guid userId);
}
