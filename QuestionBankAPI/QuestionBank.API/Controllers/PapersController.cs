using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionBank.Application.DTOs;
using QuestionBank.Application.Services;
using System.Security.Claims;

namespace QuestionBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PapersController : ControllerBase
{
    private readonly IPaperService _paperService;

    public PapersController(IPaperService paperService)
    {
        _paperService = paperService;
    }

    /// <summary>
    /// 获取试卷基本信息
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PaperDto>>> GetPaper(Guid id)
    {
        var result = await _paperService.GetPaperByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// 获取试卷详细信息（包含题目列表）
    /// </summary>
    [HttpGet("{id}/detail")]
    public async Task<ActionResult<ApiResponse<PaperDetailDto>>> GetPaperDetail(Guid id)
    {
        var result = await _paperService.GetPaperDetailAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// 获取所有试卷（分页）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<PaperDto>>>> GetAllPapers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _paperService.GetAllPapersAsync(pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// 创建试卷（手动组卷，需要教师或管理员权限）
    /// </summary>
    /// <remarks>
    /// 创建试卷示例：
    ///
    ///     POST /api/papers
    ///     {
    ///       "title": "C#基础知识测试",
    ///       "description": "涵盖C#基础语法和面向对象编程",
    ///       "duration": 60,
    ///       "questions": [
    ///         {
    ///           "questionId": "题目ID",
    ///           "questionOrder": 1,
    ///           "score": 5
    ///         },
    ///         {
    ///           "questionId": "题目ID",
    ///           "questionOrder": 2,
    ///           "score": 10
    ///         }
    ///       ]
    ///     }
    ///
    /// 系统会自动计算总分（所有题目分数之和）
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<PaperDto>>> CreatePaper(
        [FromBody] CreatePaperDto createDto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _paperService.CreatePaperAsync(createDto, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetPaper), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// 更新试卷（需要教师或管理员权限）
    /// </summary>
    /// <remarks>
    /// 更新试卷会完全替换原有的题目列表
    /// 系统会自动重新计算总分
    /// </remarks>
    [HttpPut("{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<PaperDto>>> UpdatePaper(
        Guid id,
        [FromBody] CreatePaperDto updateDto)
    {
        var result = await _paperService.UpdatePaperAsync(id, updateDto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 删除试卷（需要教师或管理员权限）
    /// </summary>
    /// <remarks>
    /// 注意：如果试卷已被用于考试，则无法删除
    /// </remarks>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePaper(Guid id)
    {
        var result = await _paperService.DeletePaperAsync(id);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
