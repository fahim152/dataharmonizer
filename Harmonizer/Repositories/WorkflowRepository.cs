using Microsoft.EntityFrameworkCore;
using Harmonizer.Data;
using Harmonizer.Models;

namespace Harmonizer.Repositories;

public class WorkflowRepository : IWorkflowRepository
{
    private readonly HarmonizerDbContext _context;

    public WorkflowRepository(HarmonizerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Workflow>> GetAllAsync()
    {
        return await _context.Workflows
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task<Workflow?> GetByIdAsync(int id)
    {
        return await _context.Workflows.FindAsync(id);
    }

    public async Task<Workflow?> GetByApiKeyAsync(string apiKey)
    {
        return await _context.Workflows
            .FirstOrDefaultAsync(w => w.ApiKey == apiKey);
    }

    public async Task<Workflow> CreateAsync(Workflow workflow)
    {
        _context.Workflows.Add(workflow);
        await _context.SaveChangesAsync();
        return workflow;
    }

    public async Task<Workflow?> UpdateAsync(Workflow workflow)
    {
        var existing = await _context.Workflows.FindAsync(workflow.Id);
        if (existing == null) return null;

        existing.Name = workflow.Name;
        existing.Description = workflow.Description;
        existing.ContextPrompt = workflow.ContextPrompt;
        existing.DesiredOutput = workflow.DesiredOutput;
        existing.DesiredOutputType = workflow.DesiredOutputType;
        existing.IsActive = workflow.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var workflow = await _context.Workflows.FindAsync(id);
        if (workflow == null) return false;

        _context.Workflows.Remove(workflow);
        await _context.SaveChangesAsync();
        return true;
    }
}
