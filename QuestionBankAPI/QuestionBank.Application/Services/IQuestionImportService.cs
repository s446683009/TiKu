using QuestionBank.Application.DTOs;

namespace QuestionBank.Application.Services;

public interface IQuestionImportService
{
    /// <summary>
    /// 从Word文档导入题目
    /// </summary>
    Task<QuestionImportResultDto> ImportFromWordAsync(Stream stream, string fileName, Guid creatorId);

    /// <summary>
    /// 从文本文件导入题目
    /// </summary>
    Task<QuestionImportResultDto> ImportFromTextAsync(Stream stream, string fileName, Guid creatorId);

    /// <summary>
    /// 解析Word文档内容
    /// </summary>
    Task<List<QuestionImportDto>> ParseWordDocumentAsync(Stream stream);

    /// <summary>
    /// 解析文本文件内容
    /// </summary>
    Task<List<QuestionImportDto>> ParseTextFileAsync(Stream stream);

    /// <summary>
    /// 验证并保存导入的题目
    /// </summary>
    Task<QuestionImportResultDto> ValidateAndSaveQuestionsAsync(
        List<QuestionImportDto> questions,
        Guid creatorId);
}
