using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionBank.Application.DTOs;
using QuestionBank.Application.Services;
using System.Security.Claims;

namespace QuestionBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly IQuestionImportService _questionImportService;

    public QuestionsController(
        IQuestionService questionService,
        IQuestionImportService questionImportService)
    {
        _questionService = questionService;
        _questionImportService = questionImportService;
    }

    /// <summary>
    /// 获取题目详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<QuestionDto>>> GetQuestion(Guid id)
    {
        var result = await _questionService.GetQuestionByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// 搜索题目(支持分页和多条件筛选)
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<ApiResponse<PagedResponse<QuestionDto>>>> SearchQuestions(
        [FromBody] QuestionSearchDto searchDto)
    {
        var result = await _questionService.SearchQuestionsAsync(searchDto);
        return Ok(result);
    }

    /// <summary>
    /// 创建题目(需要教师或管理员权限)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<QuestionDto>>> CreateQuestion(
        [FromBody] CreateQuestionDto createDto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _questionService.CreateQuestionAsync(createDto, userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetQuestion), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// 更新题目(需要教师或管理员权限)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<QuestionDto>>> UpdateQuestion(
        Guid id, [FromBody] UpdateQuestionDto updateDto)
    {
        if (id != updateDto.Id)
        {
            return BadRequest(ApiResponse<QuestionDto>.ErrorResponse("ID不匹配"));
        }

        var result = await _questionService.UpdateQuestionAsync(updateDto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// 删除题目(需要教师或管理员权限)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteQuestion(Guid id)
    {
        var result = await _questionService.DeleteQuestionAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// 根据知识点获取题目
    /// </summary>
    [HttpGet("by-knowledge-point/{knowledgePointId}")]
    public async Task<ActionResult<ApiResponse<List<QuestionDto>>>> GetQuestionsByKnowledgePoint(
        Guid knowledgePointId)
    {
        var result = await _questionService.GetQuestionsByKnowledgePointAsync(knowledgePointId);
        return Ok(result);
    }

    /// <summary>
    /// 从Word文档批量导入题目(需要教师或管理员权限)
    /// </summary>
    /// <remarks>
    /// 文件格式说明：
    ///
    /// 题型: 单选题
    /// 题干: 这是一个问题？
    /// 选项: A. 选项1 | B. 选项2 | C. 选项3 | D. 选项4
    /// 答案: A
    /// 解析: 这是解析
    /// 难度: 3
    /// 分数: 2
    /// 章节: 第一章
    /// 知识点: 知识点1, 知识点2
    /// ---
    ///
    /// 每个题目之间用"---"分隔
    /// 必填字段：题型、题干、答案
    /// 题型支持：单选题、多选题、判断题、填空题、简答题、材料题
    /// 难度支持：1-5（1最易，5最难）或文字（容易、中等、困难等）
    /// </remarks>
    [HttpPost("import/word")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<QuestionImportResultDto>>> ImportFromWord(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<QuestionImportResultDto>.ErrorResponse("文件不能为空"));
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            using var stream = file.OpenReadStream();
            var result = await _questionImportService.ImportFromWordAsync(stream, file.FileName, userId);

            return Ok(ApiResponse<QuestionImportResultDto>.SuccessResponse(
                result,
                $"导入完成：成功 {result.SuccessCount} 条，失败 {result.FailedCount} 条"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<QuestionImportResultDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// 从文本文件批量导入题目(需要教师或管理员权限)
    /// </summary>
    /// <remarks>
    /// 文件格式说明：
    ///
    /// 题型: 单选题
    /// 题干: 这是一个问题？
    /// 选项: A. 选项1 | B. 选项2 | C. 选项3 | D. 选项4
    /// 答案: A
    /// 解析: 这是解析
    /// 难度: 3
    /// 分数: 2
    /// 章节: 第一章
    /// 知识点: 知识点1, 知识点2
    /// ---
    ///
    /// 每个题目之间用"---"分隔
    /// 必填字段：题型、题干、答案
    /// 题型支持：单选题、多选题、判断题、填空题、简答题、材料题
    /// 难度支持：1-5（1最易，5最难）或文字（容易、中等、困难等）
    /// </remarks>
    [HttpPost("import/text")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<QuestionImportResultDto>>> ImportFromText(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<QuestionImportResultDto>.ErrorResponse("文件不能为空"));
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            using var stream = file.OpenReadStream();
            var result = await _questionImportService.ImportFromTextAsync(stream, file.FileName, userId);

            return Ok(ApiResponse<QuestionImportResultDto>.SuccessResponse(
                result,
                $"导入完成：成功 {result.SuccessCount} 条，失败 {result.FailedCount} 条"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<QuestionImportResultDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// 下载题目导入模板
    /// </summary>
    [HttpGet("import/template")]
    [Authorize(Roles = "Teacher,Admin")]
    public IActionResult DownloadTemplate()
    {
        var template = @"题型: 单选题
题干: C# 中哪个关键字用于定义类？
选项: A. class | B. struct | C. interface | D. enum
答案: A
解析: class 关键字用于定义类，这是面向对象编程的基础。
难度: 1
分数: 2
章节: 第一章 C#基础
知识点: C#语法, 面向对象
---
题型: 多选题
题干: 以下哪些是值类型？
选项: A. int | B. string | C. bool | D. struct
答案: A,C,D
解析: int、bool和struct都是值类型，string是引用类型。
难度: 2
分数: 3
章节: 第二章 数据类型
知识点: 数据类型, 值类型与引用类型
---
题型: 判断题
题干: C# 是一种面向对象的编程语言。
答案: 正确
解析: C# 是由微软开发的面向对象编程语言。
难度: 1
分数: 1
章节: 第一章 C#基础
知识点: C#概述
---";

        var bytes = System.Text.Encoding.UTF8.GetBytes(template);
        return File(bytes, "text/plain", "题目导入模板.txt");
    }
}
