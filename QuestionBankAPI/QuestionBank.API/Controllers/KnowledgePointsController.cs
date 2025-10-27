using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionBank.Application.DTOs;
using QuestionBank.Application.Services;

namespace QuestionBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KnowledgePointsController : ControllerBase
{
    private readonly IKnowledgePointService _knowledgePointService;

    public KnowledgePointsController(IKnowledgePointService knowledgePointService)
    {
        _knowledgePointService = knowledgePointService;
    }

    /// <summary>
    /// 获取知识点详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<KnowledgePointDto>>> GetKnowledgePoint(Guid id)
    {
        var result = await _knowledgePointService.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// 获取所有知识点
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<KnowledgePointDto>>>> GetAllKnowledgePoints()
    {
        var result = await _knowledgePointService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// 获取根级知识点
    /// </summary>
    [HttpGet("roots")]
    public async Task<ActionResult<ApiResponse<List<KnowledgePointDto>>>> GetRootKnowledgePoints()
    {
        var result = await _knowledgePointService.GetRootKnowledgePointsAsync();
        return Ok(result);
    }

    /// <summary>
    /// 获取子知识点
    /// </summary>
    [HttpGet("{parentId}/children")]
    public async Task<ActionResult<ApiResponse<List<KnowledgePointDto>>>> GetChildren(Guid parentId)
    {
        var result = await _knowledgePointService.GetChildrenAsync(parentId);
        return Ok(result);
    }

    /// <summary>
    /// 获取知识点树(包含所有子节点)
    /// </summary>
    [HttpGet("{id}/tree")]
    public async Task<ActionResult<ApiResponse<KnowledgePointDto>>> GetKnowledgeTree(Guid id)
    {
        var result = await _knowledgePointService.GetKnowledgeTreeAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// 创建知识点(教师/管理员)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<KnowledgePointDto>>> CreateKnowledgePoint(
        [FromBody] CreateKnowledgePointDto createDto)
    {
        var result = await _knowledgePointService.CreateAsync(createDto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetKnowledgePoint), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// 更新知识点(教师/管理员)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<KnowledgePointDto>>> UpdateKnowledgePoint(
        Guid id,
        [FromBody] CreateKnowledgePointDto updateDto)
    {
        var result = await _knowledgePointService.UpdateAsync(id, updateDto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// 删除知识点(管理员)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteKnowledgePoint(Guid id)
    {
        var result = await _knowledgePointService.DeleteAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
