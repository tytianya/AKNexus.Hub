using MiniMESApp.Data;
using MiniMESApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniMESApp.Repositories;

public class MacAllocationRepository : IMacAllocationRepository
{
    private readonly DasAppDbContext _db;
    public MacAllocationRepository(DasAppDbContext db) => _db = db;

    public async Task<long> InsertAsync(MacAllocation allocation)
    {
        await _db.MacAllocations.AddAsync(allocation);
        await _db.SaveChangesAsync();
        return allocation.Id;
    }

    public Task<List<MacAllocation>> GetAllAsync()
        => _db.MacAllocations.OrderByDescending(a => a.AllocatedAt).ToListAsync();
}
