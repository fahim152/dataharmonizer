using Harmonizer.DTOs;
using Harmonizer.Models;
using Harmonizer.Repositories;
using Harmonizer.Services.LLM;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Harmonizer.Services;

public class HarmonizedRecordService : IHarmonizedRecordService
{
    private readonly IHarmonizedRecordRepository _recordRepo;
    private readonly IHarmonizeRequestRepository _requestRepo;
    private readonly ILLMClient _llmClient;

    public HarmonizedRecordService(IHarmonizedRecordRepository recordRepo, IHarmonizeRequestRepository requestRepo, ILLMClient llmClient)
    {
        _recordRepo = recordRepo;
        _requestRepo = requestRepo;
        _llmClient = llmClient;
    }

    public async Task<HarmonizedRecordResponseDto?> GetByIdAsync(int id)
    {
        var entity = await _recordRepo.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<List<HarmonizedRecordResponseDto>> GetAllAsync()
    {
        var list = await _recordRepo.GetAllAsync();
        return list.ConvertAll(MapToDto);
    }

    public async Task<List<HarmonizedRecordResponseDto>> GetByRequestIdAsync(int harmonizeRequestId)
    {
        var list = await _recordRepo.GetByRequestIdAsync(harmonizeRequestId);
        return list.ConvertAll(MapToDto);
    }

    public async Task<HarmonizedRecordResponseDto> CreateAsync(HarmonizedRecordCreateDto dto)
    {
        var entity = new HarmonizedRecord
        {
            RawJson = dto.RawJson,
            Source = dto.Source,
            HarmonizeRequestId = dto.HarmonizeRequestId,
            ReceivedAt = DateTime.UtcNow
        };
        var created = await _recordRepo.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task UpdateAsync(int id, HarmonizedRecordCreateDto dto)
    {
        var entity = await _recordRepo.GetByIdAsync(id);
        if (entity == null) return;
        entity.RawJson = dto.RawJson;
        entity.Source = dto.Source;
        await _recordRepo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id) => await _recordRepo.DeleteAsync(id);

    public async Task<HarmonizedRecordResponseDto> HarmonizeAsync(int requestId, string contextPrompt, string? desiredSchema)
    {
        var request = await _requestRepo.GetByIdAsync(requestId);
        if (request == null) throw new KeyNotFoundException();

        var prompt = BuildPrompt(request.Payload, contextPrompt, desiredSchema);
        var llmResult = await _llmClient.CompleteAsync(prompt);
        var record = new HarmonizedRecord
        {
            RawJson = request.Payload,
            Source = request.Source,
            HarmonizedJson = llmResult,
            HarmonizeRequestId = request.Id,
            ReceivedAt = DateTime.UtcNow
        };
        var created = await _recordRepo.AddAsync(record);
        return MapToDto(created);
    }

    private static string BuildPrompt(string payload, string contextPrompt, string? desiredSchema)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(contextPrompt);
        sb.AppendLine();
        sb.AppendLine("Payload:");
        sb.AppendLine(payload);
        if (!string.IsNullOrWhiteSpace(desiredSchema))
        {
            sb.AppendLine();
            sb.AppendLine("Desired Schema:");
            sb.AppendLine(desiredSchema);
        }
        return sb.ToString();
    }

    private static HarmonizedRecordResponseDto MapToDto(HarmonizedRecord entity) => new()
    {
        Id = entity.Id,
        RawJson = entity.RawJson,
        Source = entity.Source,
        HarmonizedJson = entity.HarmonizedJson,
        HarmonizeRequestId = entity.HarmonizeRequestId,
        ReceivedAt = entity.ReceivedAt
    };
}
