using MiniMESApp.Data;
using MiniMESApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniMESApp.Repositories;

public class MacRangeRepository : IMacRangeRepository
{
    private readonly DasAppDbContext _db;
    public MacRangeRepository(DasAppDbContext db) => _db = db;

    public Task<List<MacRange>> GetAllAsync()
        => _db.MacRanges.OrderBy(r => r.Id).ToListAsync();

    public Task<MacRange?> GetById(int id)
        => _db.MacRanges.FirstOrDefaultAsync(r => r.Id == id);

    public async Task<int> InsertAsync(MacRange range)
    {
        await _db.MacRanges.AddAsync(range);
        await _db.SaveChangesAsync();
        return range.Id;
    }

    public async Task UpdateAsync(MacRange range)
    {
        _db.MacRanges.Update(range);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var range = await _db.MacRanges.FirstOrDefaultAsync(r => r.Id == id);
        if (range != null)
        {
            _db.MacRanges.Remove(range);
            await _db.SaveChangesAsync();
        }
    }
}
