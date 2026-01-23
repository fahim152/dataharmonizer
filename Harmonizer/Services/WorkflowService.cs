using System.Security.Cryptography;
using Harmonizer.DTOs;
using Harmonizer.Models;
using Harmonizer.Repositories;

namespace Harmonizer.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IWorkflowRepository _repository;
    private readonly IOutboundApiService _outboundApiService;

    public WorkflowService(IWorkflowRepository repository, IOutboundApiService outboundApiService)
    {
        _repository = repository;
        _outboundApiService = outboundApiService;
    }

    public async Task<IEnumerable<WorkflowDto>> GetAllAsync()
    {
        var workflows = await _repository.GetAllAsync();
        return workflows.Select(MapToDto);
    }

    public async Task<WorkflowDto?> GetByIdAsync(int id)
    {
        var workflow = await _repository.GetByIdAsync(id);
        return workflow == null ? null : MapToDto(workflow);
    }

    public async Task<Workflow?> GetByApiKeyAsync(string apiKey)
    {
        return await _repository.GetByApiKeyAsync(apiKey);
    }

    public async Task<WorkflowDto> CreateAsync(WorkflowCreateDto dto)
    {
        var workflow = new Workflow
        {
            Name = dto.Name,
            Description = dto.Description,
            ContextPrompt = dto.ContextPrompt,
            DesiredOutput = dto.DesiredOutput,
            DesiredOutputType = dto.DesiredOutputType,
            ApiKey = GenerateApiKey(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            TeamId = dto.TeamId,
            EntityName = dto.EntityName,
            OutboundApiEnabled = dto.OutboundApiEnabled,
            OutboundApiUrl = dto.OutboundApiUrl,
            OutboundApiHeaders = dto.OutboundApiHeaders
        };

        var created = await _repository.CreateAsync(workflow);
        return MapToDto(created);
    }

    public async Task<WorkflowDto?> UpdateAsync(int id, WorkflowUpdateDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return null;

        // Track if outbound API is being enabled
        bool wasOutboundDisabled = !existing.OutboundApiEnabled;

        if (dto.Name != null) existing.Name = dto.Name;
        if (dto.Description != null) existing.Description = dto.Description;
        if (dto.ContextPrompt != null) existing.ContextPrompt = dto.ContextPrompt;
        if (dto.DesiredOutput != null) existing.DesiredOutput = dto.DesiredOutput;
        if (dto.DesiredOutputType != null) existing.DesiredOutputType = dto.DesiredOutputType;
        if (dto.IsActive.HasValue) existing.IsActive = dto.IsActive.Value;
        
        // Always update TeamId even if null (allows clearing team assignment)
        existing.TeamId = dto.TeamId;
        
        // Always update EntityName even if null/empty (allows clearing entity name)
        if (dto.EntityName != null) existing.EntityName = dto.EntityName;
        
        // Outbound API configuration
        if (dto.OutboundApiEnabled.HasValue) existing.OutboundApiEnabled = dto.OutboundApiEnabled.Value;
        if (dto.OutboundApiUrl != null) existing.OutboundApiUrl = dto.OutboundApiUrl;
        if (dto.OutboundApiHeaders != null) existing.OutboundApiHeaders = dto.OutboundApiHeaders;
        
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        
        // If outbound API was just enabled, trigger sending unsent records in background
        if (wasOutboundDisabled && existing.OutboundApiEnabled && !string.IsNullOrEmpty(existing.OutboundApiUrl))
        {
            // Fire and forget - send unsent records in background
            _ = Task.Run(async () => 
            {
                await _outboundApiService.SendUnsentRecordsAsync(id);
            });
        }
        
        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<string> RegenerateApiKeyAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) throw new KeyNotFoundException($"Workflow {id} not found");

        existing.ApiKey = GenerateApiKey();
        await _repository.UpdateAsync(existing);
        return existing.ApiKey;
    }

    private static string GenerateApiKey()
    {
        var bytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "")[..32];
    }

    private static WorkflowDto MapToDto(Workflow workflow) => new()
    {
        Id = workflow.Id,
        Name = workflow.Name,
        Description = workflow.Description,
        ContextPrompt = workflow.ContextPrompt,
        DesiredOutput = workflow.DesiredOutput,
        DesiredOutputType = workflow.DesiredOutputType,
        ApiKey = workflow.ApiKey,
        IsActive = workflow.IsActive,
        CreatedAt = workflow.CreatedAt,
        UpdatedAt = workflow.UpdatedAt,
        TeamId = workflow.TeamId,
        EntityName = workflow.EntityName,
        OutboundApiEnabled = workflow.OutboundApiEnabled,
        OutboundApiUrl = workflow.OutboundApiUrl,
        OutboundApiHeaders = workflow.OutboundApiHeaders
    };
}
