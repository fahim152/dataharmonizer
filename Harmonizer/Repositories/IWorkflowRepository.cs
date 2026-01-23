using Harmonizer.Models;

namespace Harmonizer.Repositories;

public interface IWorkflowRepository
{
    Task<IEnumerable<Workflow>> GetAllAsync();
    Task<Workflow?> GetByIdAsync(int id);
    Task<Workflow?> GetByApiKeyAsync(string apiKey);
    Task<Workflow> CreateAsync(Workflow workflow);
    Task<Workflow?> UpdateAsync(Workflow workflow);
    Task<bool> DeleteAsync(int id);
}
