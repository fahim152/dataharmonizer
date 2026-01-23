using Harmonizer.Data;
using Harmonizer.Models;
using Microsoft.EntityFrameworkCore;

namespace Harmonizer.Repositories;

public interface IPromptRepository
{
    Task<Prompt?> GetByIdAsync(int id);
    Task<List<Prompt>> GetAllAsync();
    Task<Prompt> AddAsync(Prompt prompt);
    Task UpdateAsync(Prompt prompt);
    Task DeleteAsync(int id);
}

public class PromptRepository : IPromptRepository
{
    private readonly HarmonizerDbContext _context;

    public PromptRepository(HarmonizerDbContext context) => _context = context;

    public async Task<Prompt?> GetByIdAsync(int id) =>
        await _context.Prompts.FindAsync(id);

    public async Task<List<Prompt>> GetAllAsync() =>
        await _context.Prompts.OrderBy(p => p.Name).ToListAsync();

    public async Task<Prompt> AddAsync(Prompt prompt)
    {
        _context.Prompts.Add(prompt);
        await _context.SaveChangesAsync();
        return prompt;
    }

    public async Task UpdateAsync(Prompt prompt)
    {
        _context.Prompts.Update(prompt);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var prompt = await GetByIdAsync(id);
        if (prompt != null)
        {
            _context.Prompts.Remove(prompt);
            await _context.SaveChangesAsync();
        }
    }
}

