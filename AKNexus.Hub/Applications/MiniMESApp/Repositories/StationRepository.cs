using MiniMESApp.Data;
using MiniMESApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniMESApp.Repositories;

public class StationRepository : IStationRepository
{
    private readonly DasAppDbContext _db;
    public StationRepository(DasAppDbContext db) => _db = db;

    public Task<Station?> GetByCode(string code)
        => _db.Stations.FirstOrDefaultAsync(s => s.StationCode == code);

    public Task<Station?> GetById(int id)
        => _db.Stations.FirstOrDefaultAsync(s => s.Id == id);

    public Task<List<Station>> GetByIdsAsync(List<long> ids)
        => _db.Stations.Where(s => ids.Contains(s.Id)).ToListAsync();

    public Task<List<Station>> GetAllAsync()
        => _db.Stations.OrderBy(s => s.SortOrder).ToListAsync();

    public async Task<int> InsertAsync(Station station)
    {
        await _db.Stations.AddAsync(station);
        await _db.SaveChangesAsync();
        return station.Id;
    }

    public async Task UpdateAsync(Station station)
    {
        _db.Stations.Update(station);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var station = await _db.Stations.FirstOrDefaultAsync(s => s.Id == id);
        if (station != null)
        {
            _db.Stations.Remove(station);
            await _db.SaveChangesAsync();
        }
    }
}
