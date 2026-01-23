using Harmonizer.DTOs;
using Harmonizer.Models;

namespace Harmonizer.Services;

public interface IWorkflowService
{
    Task<IEnumerable<WorkflowDto>> GetAllAsync();
    Task<WorkflowDto?> GetByIdAsync(int id);
    Task<Workflow?> GetByApiKeyAsync(string apiKey);
    Task<WorkflowDto> CreateAsync(WorkflowCreateDto dto);
    Task<WorkflowDto?> UpdateAsync(int id, WorkflowUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<string> RegenerateApiKeyAsync(int id);
}
