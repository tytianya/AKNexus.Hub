using MiniMESApp.Data;
using MiniMESApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniMESApp.Repositories;

public class GateRuleRepository : IGateRuleRepository
{
    private readonly DasAppDbContext _db;
    public GateRuleRepository(DasAppDbContext db) => _db = db;

    public Task<List<GateRule>> GetActiveByStation(int stationId)
        => _db.GateRules
            .Where(r => r.StationId == stationId && r.IsActive)
            .OrderByDescending(r => r.Priority)
            .ToListAsync();

    public Task<List<GateRule>> GetAllAsync()
        => _db.GateRules.OrderBy(r => r.StationId).ThenByDescending(r => r.Priority).ToListAsync();

    public Task<GateRule?> GetById(int id)
        => _db.GateRules.FirstOrDefaultAsync(r => r.Id == id);

    public async Task<int> InsertAsync(GateRule rule)
    {
        await _db.GateRules.AddAsync(rule);
        await _db.SaveChangesAsync();
        return rule.Id;
    }

    public async Task UpdateAsync(GateRule rule)
    {
        _db.GateRules.Update(rule);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var rule = await _db.GateRules.FirstOrDefaultAsync(r => r.Id == id);
        if (rule != null)
        {
            _db.GateRules.Remove(rule);
            await _db.SaveChangesAsync();
        }
    }
}
