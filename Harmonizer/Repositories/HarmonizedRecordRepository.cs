using Harmonizer.Data;
using Harmonizer.Models;
using Microsoft.EntityFrameworkCore;

namespace Harmonizer.Repositories;

public class HarmonizedRecordRepository : IHarmonizedRecordRepository
{
    private readonly HarmonizerDbContext _context;
    public HarmonizedRecordRepository(HarmonizerDbContext context) => _context = context;

    public async Task<HarmonizedRecord?> GetByIdAsync(int id) =>
        await _context.HarmonizedRecords.Include(r => r.HarmonizeRequest).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<HarmonizedRecord>> GetAllAsync() =>
        await _context.HarmonizedRecords.Include(r => r.HarmonizeRequest).ToListAsync();

    public async Task<List<HarmonizedRecord>> GetByRequestIdAsync(int harmonizeRequestId) =>
        await _context.HarmonizedRecords.Where(r => r.HarmonizeRequestId == harmonizeRequestId).ToListAsync();

    public async Task<HarmonizedRecord> AddAsync(HarmonizedRecord record)
    {
        _context.HarmonizedRecords.Add(record);
        await _context.SaveChangesAsync();
        return record;
    }

    public async Task UpdateAsync(HarmonizedRecord record)
    {
        _context.HarmonizedRecords.Update(record);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.HarmonizedRecords.FindAsync(id);
        if (entity != null)
        {
            _context.HarmonizedRecords.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}

