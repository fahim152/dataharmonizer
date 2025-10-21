using Harmonizer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Harmonizer.Repositories;

public interface IHarmonizedRecordRepository
{
    Task<HarmonizedRecord?> GetByIdAsync(int id);
    Task<List<HarmonizedRecord>> GetAllAsync();
    Task<List<HarmonizedRecord>> GetByRequestIdAsync(int harmonizeRequestId);
    Task<HarmonizedRecord> AddAsync(HarmonizedRecord record);
    Task UpdateAsync(HarmonizedRecord record);
    Task DeleteAsync(int id);
}

