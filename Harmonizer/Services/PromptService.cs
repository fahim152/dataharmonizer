using Harmonizer.DTOs;
using Harmonizer.Models;
using Harmonizer.Repositories;

namespace Harmonizer.Services;

public interface IPromptService
{
    Task<PromptResponseDto?> GetByIdAsync(int id);
    Task<List<PromptResponseDto>> GetAllAsync();
    Task<PromptResponseDto> CreateAsync(PromptCreateDto dto);
    Task UpdateAsync(int id, PromptCreateDto dto);
    Task DeleteAsync(int id);
}

public class PromptService : IPromptService
{
    private readonly IPromptRepository _repo;

    public PromptService(IPromptRepository repo) => _repo = repo;

    public async Task<PromptResponseDto?> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<List<PromptResponseDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.ConvertAll(MapToDto);
    }

    public async Task<PromptResponseDto> CreateAsync(PromptCreateDto dto)
    {
        var entity = new Prompt
        {
            Name = dto.Name,
            ContextPrompt = dto.ContextPrompt,
            DesiredOutput = dto.DesiredOutput,
            DesiredOutputType = dto.DesiredOutputType,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };
        var created = await _repo.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task UpdateAsync(int id, PromptCreateDto dto)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return;
        
        entity.Name = dto.Name;
        entity.ContextPrompt = dto.ContextPrompt;
        entity.DesiredOutput = dto.DesiredOutput;
        entity.DesiredOutputType = dto.DesiredOutputType;
        entity.Description = dto.Description;
        
        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id) => await _repo.DeleteAsync(id);

    private static PromptResponseDto MapToDto(Prompt entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        ContextPrompt = entity.ContextPrompt,
        DesiredOutput = entity.DesiredOutput,
        DesiredOutputType = entity.DesiredOutputType,
        Description = entity.Description,
        CreatedAt = entity.CreatedAt
    };
}

