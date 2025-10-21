using Harmonizer.DTOs;
using Harmonizer.Services;
using Microsoft.AspNetCore.Mvc;

namespace Harmonizer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HarmonizeRequestsController : ControllerBase
{
    private readonly IHarmonizeRequestService _service;
    public HarmonizeRequestsController(IHarmonizeRequestService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<HarmonizeRequestResponseDto>>> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<HarmonizeRequestResponseDto>> Get(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<HarmonizeRequestResponseDto>> Create([FromBody] HarmonizeRequestCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] HarmonizeRequestCreateDto dto)
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
}

