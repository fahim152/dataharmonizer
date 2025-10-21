using Harmonizer.DTOs;
using Harmonizer.Models;
using Harmonizer.Repositories;

namespace Harmonizer.Services;

public class HarmonizeRequestService : IHarmonizeRequestService
{
    private readonly IHarmonizeRequestRepository _repo;
    public HarmonizeRequestService(IHarmonizeRequestRepository repo) => _repo = repo;

    public async Task<HarmonizeRequestResponseDto?> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null : new HarmonizeRequestResponseDto
        {
            Id = entity.Id,
            Payload = entity.Payload,
            Source = entity.Source,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task<List<HarmonizeRequestResponseDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.ConvertAll(entity => new HarmonizeRequestResponseDto
        {
            Id = entity.Id,
            Payload = entity.Payload,
            Source = entity.Source,
            CreatedAt = entity.CreatedAt
        });
    }

    public async Task<HarmonizeRequestResponseDto> CreateAsync(HarmonizeRequestCreateDto dto)
    {
        var entity = new HarmonizeRequest
        {
            Payload = dto.Payload,
            Source = dto.Source
        };
        var created = await _repo.AddAsync(entity);
        return new HarmonizeRequestResponseDto
        {
            Id = created.Id,
            Payload = created.Payload,
            Source = created.Source,
            CreatedAt = created.CreatedAt
        };
    }

    public async Task UpdateAsync(int id, HarmonizeRequestCreateDto dto)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return;
        entity.Payload = dto.Payload;
        entity.Source = dto.Source;
        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id) => await _repo.DeleteAsync(id);
}



public interface IHarmonizeRequestService
{
    Task<HarmonizeRequestResponseDto?> GetByIdAsync(int id);
    Task<List<HarmonizeRequestResponseDto>> GetAllAsync();
    Task<HarmonizeRequestResponseDto> CreateAsync(HarmonizeRequestCreateDto dto);
    Task UpdateAsync(int id, HarmonizeRequestCreateDto dto);
    Task DeleteAsync(int id);
}

