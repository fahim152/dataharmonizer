using Harmonizer.DTOs;
using Harmonizer.Services;
using Microsoft.AspNetCore.Mvc;

namespace Harmonizer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HarmonizedRecordsController : ControllerBase
{
    private readonly IHarmonizedRecordService _service;
    public HarmonizedRecordsController(IHarmonizedRecordService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<HarmonizedRecordResponseDto>>> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<HarmonizedRecordResponseDto>> Get(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("by-request/{requestId}")]
    public async Task<ActionResult<List<HarmonizedRecordResponseDto>>> GetByRequestId(int requestId) => Ok(await _service.GetByRequestIdAsync(requestId));

    [HttpGet("by-workflow/{workflowId}")]
    public async Task<ActionResult<List<HarmonizedRecordResponseDto>>> GetByWorkflowId(int workflowId)
    {
        var records = await _service.GetByWorkflowIdAsync(workflowId);
        return Ok(records);
    }

    [HttpPost]
    public async Task<ActionResult<HarmonizedRecordResponseDto>> Create([FromBody] HarmonizedRecordCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] HarmonizedRecordCreateDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("harmonize/{requestId}")]
    public async Task<ActionResult<HarmonizedRecordResponseDto>> Harmonize(int requestId, [FromBody] HarmonizeRecordRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ContextPrompt))
            return BadRequest("ContextPrompt is required.");
        var result = await _service.HarmonizeAsync(requestId, dto.ContextPrompt, dto.DesiredSchema);
        return Ok(result);
    }
}


