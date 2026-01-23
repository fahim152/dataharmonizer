using Harmonizer.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Harmonizer.Services;

public interface IHarmonizedRecordService
{
    Task<HarmonizedRecordResponseDto?> GetByIdAsync(int id);
    Task<List<HarmonizedRecordResponseDto>> GetAllAsync();
    Task<List<HarmonizedRecordResponseDto>> GetByRequestIdAsync(int harmonizeRequestId);
    Task<List<HarmonizedRecordResponseDto>> GetByWorkflowIdAsync(int workflowId);
    Task<HarmonizedRecordResponseDto> CreateAsync(HarmonizedRecordCreateDto dto);
    Task UpdateAsync(int id, HarmonizedRecordCreateDto dto);
    Task DeleteAsync(int id);
    Task<HarmonizedRecordResponseDto> HarmonizeAsync(int requestId, string contextPrompt, string? desiredSchema);
}

