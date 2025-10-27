using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestionBank.Application.DTOs;
using QuestionBank.Domain.Entities;
using QuestionBank.Domain.Enums;
using QuestionBank.Infrastructure.Repositories;

namespace QuestionBank.Application.Services;

public class QuestionImportService : IQuestionImportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQuestionRepository _questionRepository;
    private readonly IRepository<KnowledgePoint> _knowledgePointRepository;

    public QuestionImportService(
        IUnitOfWork unitOfWork,
        IQuestionRepository questionRepository,
        IRepository<KnowledgePoint> knowledgePointRepository)
    {
        _unitOfWork = unitOfWork;
        _questionRepository = questionRepository;
        _knowledgePointRepository = knowledgePointRepository;
    }

    public async Task<QuestionImportResultDto> ImportFromWordAsync(Stream stream, string fileName, Guid creatorId)
    {
        if (stream == null || stream.Length == 0)
        {
            throw new ArgumentException("文件流不能为空");
        }

        if (!fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("只支持.docx格式的Word文档");
        }

        var questions = await ParseWordDocumentAsync(stream);
        return await ValidateAndSaveQuestionsAsync(questions, creatorId);
    }

    public async Task<QuestionImportResultDto> ImportFromTextAsync(Stream stream, string fileName, Guid creatorId)
    {
        if (stream == null || stream.Length == 0)
        {
            throw new ArgumentException("文件流不能为空");
        }

        if (!fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("只支持.txt格式的文本文件");
        }

        var questions = await ParseTextFileAsync(stream);
        return await ValidateAndSaveQuestionsAsync(questions, creatorId);
    }

    public async Task<List<QuestionImportDto>> ParseWordDocumentAsync(Stream stream)
    {
        var questions = new List<QuestionImportDto>();

        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart?.Document.Body;

        if (body == null)
        {
            throw new InvalidOperationException("无法读取Word文档内容");
        }

        // 提取所有段落文本
        var paragraphs = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>();
        var textBuilder = new StringBuilder();

        foreach (var para in paragraphs)
        {
            var text = para.InnerText;
            if (!string.IsNullOrWhiteSpace(text))
            {
                textBuilder.AppendLine(text);
            }
        }

        var fullText = textBuilder.ToString();

        // 使用统一的解析逻辑
        questions = ParseQuestionText(fullText);

        return await Task.FromResult(questions);
    }

    public async Task<List<QuestionImportDto>> ParseTextFileAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var content = await reader.ReadToEndAsync();

        return ParseQuestionText(content);
    }

    private List<QuestionImportDto> ParseQuestionText(string content)
    {
        var questions = new List<QuestionImportDto>();

        // 按 "---" 分隔题目
        var questionBlocks = content.Split(new[] { "---" }, StringSplitOptions.RemoveEmptyEntries);

        int lineNumber = 1;

        foreach (var block in questionBlocks)
        {
            if (string.IsNullOrWhiteSpace(block))
            {
                continue;
            }

            try
            {
                var question = ParseSingleQuestion(block, lineNumber);
                if (question != null)
                {
                    questions.Add(question);
                }
            }
            catch (Exception ex)
            {
                // 解析失败时，创建一个包含错误信息的题目
                questions.Add(new QuestionImportDto
                {
                    LineNumber = lineNumber,
                    Content = $"解析失败: {ex.Message}"
                });
            }

            lineNumber += block.Split('\n').Length;
        }

        return questions;
    }

    private QuestionImportDto? ParseSingleQuestion(string block, int lineNumber)
    {
        var lines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        if (lines.Count == 0)
        {
            return null;
        }

        var question = new QuestionImportDto
        {
            LineNumber = lineNumber,
            Difficulty = DifficultyLevel.Medium,
            Score = 1
        };

        foreach (var line in lines)
        {
            // 使用正则表达式匹配键值对
            var match = Regex.Match(line, @"^([^:：]+)[：:]\s*(.+)$");

            if (!match.Success)
            {
                continue;
            }

            var key = match.Groups[1].Value.Trim();
            var value = match.Groups[2].Value.Trim();

            switch (key)
            {
                case "题型":
                case "类型":
                    question.Type = ParseQuestionType(value);
                    break;

                case "题干":
                case "问题":
                case "题目":
                    question.Content = value;
                    break;

                case "选项":
                    question.Options = value;
                    break;

                case "答案":
                    question.CorrectAnswer = value;
                    break;

                case "解析":
                case "说明":
                    question.Explanation = value;
                    break;

                case "难度":
                    question.Difficulty = ParseDifficulty(value);
                    break;

                case "分数":
                case "分值":
                    if (int.TryParse(value, out var score))
                    {
                        question.Score = score;
                    }
                    break;

                case "章节":
                    question.Chapter = value;
                    break;

                case "知识点":
                    question.KnowledgePointNames = value
                        .Split(new[] { ',', '，', ';', '；' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(kp => kp.Trim())
                        .Where(kp => !string.IsNullOrWhiteSpace(kp))
                        .ToList();
                    break;
            }
        }

        // 验证必填字段
        if (string.IsNullOrWhiteSpace(question.Content))
        {
            throw new InvalidOperationException("题干不能为空");
        }

        if (string.IsNullOrWhiteSpace(question.CorrectAnswer))
        {
            throw new InvalidOperationException("答案不能为空");
        }

        return question;
    }

    private QuestionType ParseQuestionType(string type)
    {
        return type.ToLower() switch
        {
            "单选" or "单选题" or "single" or "singlechoice" => QuestionType.SingleChoice,
            "多选" or "多选题" or "multiple" or "multiplechoice" => QuestionType.MultipleChoice,
            "判断" or "判断题" or "truefalse" or "bool" => QuestionType.TrueFalse,
            "填空" or "填空题" or "fillin" or "blank" => QuestionType.FillBlank,
            "简答" or "简答题" or "essay" or "shortanswer" => QuestionType.ShortAnswer,
            "材料" or "材料题" or "comprehension" => QuestionType.Material,
            _ => QuestionType.SingleChoice
        };
    }

    private DifficultyLevel ParseDifficulty(string difficulty)
    {
        // 支持数字和文字两种格式
        if (int.TryParse(difficulty, out var level))
        {
            return level switch
            {
                1 => DifficultyLevel.VeryEasy,
                2 => DifficultyLevel.Easy,
                3 => DifficultyLevel.Medium,
                4 => DifficultyLevel.Hard,
                5 => DifficultyLevel.VeryHard,
                _ => DifficultyLevel.Medium
            };
        }

        return difficulty.ToLower() switch
        {
            "很容易" or "非常简单" or "veryeasy" => DifficultyLevel.VeryEasy,
            "容易" or "简单" or "easy" => DifficultyLevel.Easy,
            "中等" or "一般" or "medium" => DifficultyLevel.Medium,
            "困难" or "难" or "hard" => DifficultyLevel.Hard,
            "很困难" or "非常难" or "veryhard" => DifficultyLevel.VeryHard,
            _ => DifficultyLevel.Medium
        };
    }

    public async Task<QuestionImportResultDto> ValidateAndSaveQuestionsAsync(
        List<QuestionImportDto> questions,
        Guid creatorId)
    {
        var result = new QuestionImportResultDto
        {
            TotalCount = questions.Count
        };

        // 获取所有知识点（用于匹配）
        var allKnowledgePoints = await _knowledgePointRepository.GetAllAsync();
        var knowledgePointDict = allKnowledgePoints.ToDictionary(kp => kp.Name, kp => kp);

        foreach (var questionDto in questions)
        {
            try
            {
                // 验证题目
                ValidateQuestion(questionDto);

                // 创建题目实体
                var question = new Question
                {
                    Id = Guid.NewGuid(),
                    Type = questionDto.Type,
                    Content = questionDto.Content,
                    Options = questionDto.Options,
                    CorrectAnswer = questionDto.CorrectAnswer,
                    Explanation = questionDto.Explanation,
                    Difficulty = questionDto.Difficulty,
                    Score = questionDto.Score,
                    Chapter = questionDto.Chapter,
                    Status = QuestionStatus.Enabled,
                    CreatorId = creatorId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // 关联知识点
                foreach (var kpName in questionDto.KnowledgePointNames)
                {
                    if (knowledgePointDict.TryGetValue(kpName, out var knowledgePoint))
                    {
                        question.QuestionKnowledgePoints.Add(new QuestionKnowledgePoint
                        {
                            QuestionId = question.Id,
                            KnowledgePointId = knowledgePoint.Id
                        });
                    }
                    else
                    {
                        // 如果知识点不存在，创建新的知识点
                        var newKnowledgePoint = new KnowledgePoint
                        {
                            Id = Guid.NewGuid(),
                            Name = kpName,
                            Level = 1,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _knowledgePointRepository.AddAsync(newKnowledgePoint);
                        knowledgePointDict[kpName] = newKnowledgePoint;

                        question.QuestionKnowledgePoints.Add(new QuestionKnowledgePoint
                        {
                            QuestionId = question.Id,
                            KnowledgePointId = newKnowledgePoint.Id
                        });
                    }
                }

                // 保存题目
                await _questionRepository.AddAsync(question);

                result.SuccessCount++;
                result.ImportedQuestionIds.Add(question.Id);
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.Errors.Add(new QuestionImportErrorDto
                {
                    LineNumber = questionDto.LineNumber,
                    QuestionContent = questionDto.Content ?? "无法解析题目内容",
                    ErrorMessage = ex.Message
                });
            }
        }

        // 保存所有更改
        if (result.SuccessCount > 0)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return result;
    }

    private void ValidateQuestion(QuestionImportDto question)
    {
        if (string.IsNullOrWhiteSpace(question.Content))
        {
            throw new ArgumentException("题干不能为空");
        }

        if (string.IsNullOrWhiteSpace(question.CorrectAnswer))
        {
            throw new ArgumentException("答案不能为空");
        }

        // 对于选择题，验证选项
        if (question.Type == QuestionType.SingleChoice || question.Type == QuestionType.MultipleChoice)
        {
            if (string.IsNullOrWhiteSpace(question.Options))
            {
                throw new ArgumentException("选择题必须有选项");
            }
        }

        // 验证分数
        if (question.Score <= 0)
        {
            throw new ArgumentException("分数必须大于0");
        }
    }
}
