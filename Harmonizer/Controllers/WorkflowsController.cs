using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using Harmonizer.DTOs;
using Harmonizer.Models;
using Harmonizer.Services;
using Harmonizer.BackgroundServices;

namespace Harmonizer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    private readonly IHarmonizeRequestService _requestService;
    private readonly IHarmonizedRecordService _recordService;
    private readonly Channel<HarmonizationJob> _harmonizationChannel;

    public WorkflowsController(
        IWorkflowService workflowService,
        IHarmonizeRequestService requestService,
        IHarmonizedRecordService recordService,
        Channel<HarmonizationJob> harmonizationChannel)
    {
        _workflowService = workflowService;
        _requestService = requestService;
        _recordService = recordService;
        _harmonizationChannel = harmonizationChannel;
    }

    // GET: api/Workflows
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkflowDto>>> GetAll()
    {
        var workflows = await _workflowService.GetAllAsync();
        return Ok(workflows);
    }

    // GET: api/Workflows/5
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkflowDto>> GetById(int id)
    {
        var workflow = await _workflowService.GetByIdAsync(id);
        if (workflow == null) return NotFound();
        return Ok(workflow);
    }

    // POST: api/Workflows
    [HttpPost]
    public async Task<ActionResult<WorkflowDto>> Create([FromBody] WorkflowCreateDto dto)
    {
        var created = await _workflowService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/Workflows/5
    [HttpPut("{id}")]
    public async Task<ActionResult<WorkflowDto>> Update(int id, [FromBody] WorkflowUpdateDto dto)
    {
        var updated = await _workflowService.UpdateAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    // DELETE: api/Workflows/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _workflowService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // POST: api/Workflows/5/regenerate-key
    [HttpPost("{id}/regenerate-key")]
    public async Task<ActionResult<object>> RegenerateApiKey(int id)
    {
        try
        {
            var newKey = await _workflowService.RegenerateApiKeyAsync(id);
            return Ok(new { apiKey = newKey });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
    
    // GET: api/Workflows/5/table-name
    [HttpGet("{id}/table-name")]
    public async Task<ActionResult<object>> GetTableName(int id, [FromServices] IDynamicTableService dynamicTableService)
    {
        var tableName = await dynamicTableService.GetTableNameForWorkflowAsync(id);
        return Ok(new { tableName, exists = tableName != null });
    }

    // POST: api/Workflows/5/send-unsent
    [HttpPost("{id}/send-unsent")]
    public async Task<ActionResult> SendUnsentRecords(int id, [FromServices] IOutboundApiService outboundApiService)
    {
        await outboundApiService.SendUnsentRecordsAsync(id);
        return Ok(new { message = "Unsent records are being sent in background" });
    }

    // POST: api/Workflows/webhook/{apiKey}
    // This is the endpoint external systems call to send data for harmonization
    [HttpPost("webhook/{apiKey}")]
    public async Task<ActionResult<WorkflowWebhookResponseDto>> Webhook(
        string apiKey,
        [FromBody] JsonElement body)
    {
        // Find workflow by API key
        var workflow = await _workflowService.GetByApiKeyAsync(apiKey);
        if (workflow == null)
        {
            return NotFound(new WorkflowWebhookResponseDto
            {
                Success = false,
                Error = "Invalid API key or workflow not found"
            });
        }

        if (!workflow.IsActive)
        {
            return BadRequest(new WorkflowWebhookResponseDto
            {
                Success = false,
                Error = "Workflow is not active"
            });
        }

        try
        {
            // Extract payload from request
            string payloadJson = body.ToString();
            
            // Create a HarmonizeRequest to store the incoming data
            var requestDto = new HarmonizeRequestCreateDto
            {
                Payload = payloadJson,
                Source = $"Webhook",
                WorkflowId = workflow.Id
            };
            var request = await _requestService.CreateAsync(requestDto);

            // Queue the harmonization job for background processing
            await _harmonizationChannel.Writer.WriteAsync(new HarmonizationJob
            {
                Workflow = workflow,
                Request = new HarmonizeRequest
                {
                    Id = request.Id,
                    Payload = request.Payload,
                    Source = request.Source,
                    CreatedAt = request.CreatedAt
                }
            });

            // Return immediately - harmonization happens in background
            return Accepted(new WorkflowWebhookResponseDto
            {
                Success = true,
                RequestId = request.Id,
                Message = "Request accepted and queued for harmonization"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new WorkflowWebhookResponseDto
            {
                Success = false,
                Error = $"Failed to queue request: {ex.Message}"
            });
        }
    }
}
