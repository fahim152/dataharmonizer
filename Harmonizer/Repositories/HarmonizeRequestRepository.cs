using Harmonizer.Data;
using Harmonizer.Models;
using Microsoft.EntityFrameworkCore;

namespace Harmonizer.Repositories;

public class HarmonizeRequestRepository : IHarmonizeRequestRepository
{
    private readonly HarmonizerDbContext _context;
    public HarmonizeRequestRepository(HarmonizerDbContext context) => _context = context;

    public async Task<HarmonizeRequest?> GetByIdAsync(int id) =>
        await _context.HarmonizeRequests.Include(r => r.HarmonizedRecords).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<HarmonizeRequest>> GetAllAsync() =>
        await _context.HarmonizeRequests.Include(r => r.HarmonizedRecords).ToListAsync();

    public async Task<HarmonizeRequest> AddAsync(HarmonizeRequest request)
    {
        _context.HarmonizeRequests.Add(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task UpdateAsync(HarmonizeRequest request)
    {
        _context.HarmonizeRequests.Update(request);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.HarmonizeRequests.FindAsync(id);
        if (entity != null)
        {
            _context.HarmonizeRequests.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<HarmonizeRequest>> GetByWorkflowIdAsync(int workflowId) =>
        await _context.HarmonizeRequests
            .Include(r => r.HarmonizedRecords)
            .Where(r => r.WorkflowId == workflowId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(50)
            .ToListAsync();
}

public interface IHarmonizeRequestRepository
{
    Task<HarmonizeRequest?> GetByIdAsync(int id);
    Task<List<HarmonizeRequest>> GetAllAsync();
    Task<List<HarmonizeRequest>> GetByWorkflowIdAsync(int workflowId);
    Task<HarmonizeRequest> AddAsync(HarmonizeRequest request);
    Task UpdateAsync(HarmonizeRequest request);
    Task DeleteAsync(int id);
}

