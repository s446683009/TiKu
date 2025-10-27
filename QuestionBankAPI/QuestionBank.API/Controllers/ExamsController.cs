using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionBank.Application.DTOs;
using QuestionBank.Application.Services;
using System.Security.Claims;

namespace QuestionBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExamsController : ControllerBase
{
    private readonly IExamService _examService;

    public ExamsController(IExamService examService)
    {
        _examService = examService;
    }

    /// <summary>
    /// 获取考试详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ExamDto>>> GetExam(Guid id)
    {
        var result = await _examService.GetExamByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// 获取所有考试
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ExamDto>>>> GetAllExams()
    {
        var result = await _examService.GetAllExamsAsync();
        return Ok(result);
    }

    /// <summary>
    /// 创建考试(教师/管理员)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<ExamDto>>> CreateExam([FromBody] CreateExamDto createDto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _examService.CreateExamAsync(createDto, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetExam), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// 开始考试
    /// </summary>
    [HttpPost("{examId}/start")]
    public async Task<ActionResult<ApiResponse<ExamAttemptDto>>> StartExam(Guid examId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _examService.StartExamAsync(examId, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 提交单题答案
    /// </summary>
    [HttpPost("submit-answer")]
    public async Task<ActionResult<ApiResponse<bool>>> SubmitAnswer([FromBody] SubmitAnswerDto submitDto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _examService.SubmitAnswerAsync(submitDto, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 提交整个考试
    /// </summary>
    [HttpPost("attempts/{examAttemptId}/submit")]
    public async Task<ActionResult<ApiResponse<ExamAttemptDto>>> SubmitExam(Guid examAttemptId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _examService.SubmitExamAsync(examAttemptId, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 获取我的考试记录
    /// </summary>
    [HttpGet("my-attempts")]
    public async Task<ActionResult<ApiResponse<List<ExamAttemptDto>>>> GetMyExamAttempts()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _examService.GetUserExamAttemptsAsync(userId);
        return Ok(result);
    }
}
