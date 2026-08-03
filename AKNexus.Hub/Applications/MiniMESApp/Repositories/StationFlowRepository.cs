using MiniMESApp.Data;
using MiniMESApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniMESApp.Repositories;

public class StationFlowRepository : IStationFlowRepository
{
    private readonly DasAppDbContext _db;
    public StationFlowRepository(DasAppDbContext db) => _db = db;

    public Task<List<StationFlow>> GetAllAsync()
        => _db.StationFlows.OrderBy(f => f.FlowCode).ToListAsync();

    public Task<StationFlow?> GetById(int id)
        => _db.StationFlows.FirstOrDefaultAsync(f => f.Id == id);

    public Task<StationFlow?> GetByCode(string code)
        => _db.StationFlows.FirstOrDefaultAsync(f => f.FlowCode == code);

    public async Task<int> InsertAsync(StationFlow flow)
    {
        await _db.StationFlows.AddAsync(flow);
        await _db.SaveChangesAsync();
        return flow.Id;
    }

    public async Task UpdateAsync(StationFlow flow)
    {
        _db.StationFlows.Update(flow);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var flow = await _db.StationFlows.FirstOrDefaultAsync(f => f.Id == id);
        if (flow != null)
        {
            _db.StationFlows.Remove(flow);
            await _db.SaveChangesAsync();
        }
    }
}
