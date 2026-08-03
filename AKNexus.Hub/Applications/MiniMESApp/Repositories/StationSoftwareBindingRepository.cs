using MiniMESApp.Data;
using MiniMESApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniMESApp.Repositories;

public class StationSoftwareBindingRepository : IStationSoftwareBindingRepository
{
    private readonly DasAppDbContext _db;
    public StationSoftwareBindingRepository(DasAppDbContext db) => _db = db;

    public Task<List<StationSoftwareBinding>> GetByStation(int stationId)
        => _db.StationSoftwareBindings.Where(b => b.StationId == stationId).ToListAsync();

    public Task<StationSoftwareBinding?> GetByStationAndSoftware(int stationId, int softwareId)
        => _db.StationSoftwareBindings
            .FirstOrDefaultAsync(b => b.StationId == stationId && b.SoftwareId == softwareId);

    public async Task<int> InsertAsync(StationSoftwareBinding binding)
    {
        await _db.StationSoftwareBindings.AddAsync(binding);
        await _db.SaveChangesAsync();
        return binding.Id;
    }

    public async Task DeleteAsync(int bindingId)
    {
        var binding = await _db.StationSoftwareBindings.FirstOrDefaultAsync(b => b.Id == bindingId);
        if (binding != null)
        {
            _db.StationSoftwareBindings.Remove(binding);
            await _db.SaveChangesAsync();
        }
    }

    public async Task DeleteByStationAndSoftware(int stationId, int softwareId)
    {
        var bindings = await _db.StationSoftwareBindings
            .Where(b => b.StationId == stationId && b.SoftwareId == softwareId).ToListAsync();
        _db.StationSoftwareBindings.RemoveRange(bindings);
        await _db.SaveChangesAsync();
    }
}
