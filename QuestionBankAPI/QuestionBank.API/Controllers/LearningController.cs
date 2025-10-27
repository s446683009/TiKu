using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionBank.Application.DTOs;
using QuestionBank.Application.Services;
using System.Security.Claims;

namespace QuestionBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LearningController : ControllerBase
{
    private readonly ILearningService _learningService;

    public LearningController(ILearningService learningService)
    {
        _learningService = learningService;
    }

    #region 错题本

    /// <summary>
    /// 添加题目到错题本
    /// </summary>
    [HttpPost("wrong-questions")]
    public async Task<ActionResult<ApiResponse<WrongQuestionDto>>> AddWrongQuestion(
        [FromBody] AddWrongQuestionDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _learningService.AddWrongQuestionAsync(userId, dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 获取我的错题本列表（分页）
    /// </summary>
    [HttpGet("wrong-questions")]
    public async Task<ActionResult<ApiResponse<PagedResponse<WrongQuestionDto>>>> GetWrongQuestions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _learningService.GetWrongQuestionsAsync(userId, pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// 从错题本移除题目
    /// </summary>
    [HttpDelete("wrong-questions/{questionId}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveWrongQuestion(Guid questionId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _learningService.RemoveWrongQuestionAsync(userId, questionId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 按知识点获取错题
    /// </summary>
    [HttpGet("wrong-questions/by-knowledge-point/{knowledgePointId}")]
    public async Task<ActionResult<ApiResponse<List<WrongQuestionDto>>>> GetWrongQuestionsByKnowledgePoint(
        Guid knowledgePointId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _learningService.GetWrongQuestionsByKnowledgePointAsync(userId, knowledgePointId);
        return Ok(result);
    }

    #endregion

    #region 收藏

    /// <summary>
    /// 收藏题目
    /// </summary>
    [HttpPost("favorites")]
    public async Task<ActionResult<ApiResponse<FavoriteQuestionDto>>> AddFavorite(
        [FromBody] AddFavoriteDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _learningService.AddFavoriteAsync(userId, dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 获取我的收藏列表（分页）
    /// </summary>
    [HttpGet("favorites")]
    public async Task<ActionResult<ApiResponse<PagedResponse<FavoriteQuestionDto>>>> GetFavorites(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _learningService.GetFavoritesAsync(userId, pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// 取消收藏
    /// </summary>
    [HttpDelete("favorites/{questionId}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveFavorite(Guid questionId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _learningService.RemoveFavoriteAsync(userId, questionId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 更新收藏备注
    /// </summary>
    [HttpPut("favorites/{questionId}/note")]
    public async Task<ActionResult<ApiResponse<FavoriteQuestionDto>>> UpdateFavoriteNote(
        Guid questionId,
        [FromBody] UpdateFavoriteDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _learningService.UpdateFavoriteNoteAsync(userId, questionId, dto);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    #endregion

    #region 笔记

    /// <summary>
    /// 创建题目笔记
    /// </summary>
    [HttpPost("notes")]
    public async Task<ActionResult<ApiResponse<QuestionNoteDto>>> CreateNote(
        [FromBody] CreateQuestionNoteDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _learningService.CreateNoteAsync(userId, dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 获取题目的笔记
    /// </summary>
    [HttpGet("notes/by-question/{questionId}")]
    public async Task<ActionResult<ApiResponse<QuestionNoteDto>>> GetNoteByQuestionId(Guid questionId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _learningService.GetNoteByQuestionIdAsync(userId, questionId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 获取我的所有笔记（分页）
    /// </summary>
    [HttpGet("notes")]
    public async Task<ActionResult<ApiResponse<PagedResponse<QuestionNoteDto>>>> GetUserNotes(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _learningService.GetUserNotesAsync(userId, pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// 更新笔记
    /// </summary>
    [HttpPut("notes/{noteId}")]
    public async Task<ActionResult<ApiResponse<QuestionNoteDto>>> UpdateNote(
        Guid noteId,
        [FromBody] UpdateQuestionNoteDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _learningService.UpdateNoteAsync(userId, noteId, dto);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 删除笔记
    /// </summary>
    [HttpDelete("notes/{noteId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteNote(Guid noteId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _learningService.DeleteNoteAsync(userId, noteId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    #endregion
}
