using MiniMESApp.Data;
using MiniMESApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniMESApp.Repositories;

public class IncomingMaterialRepository : IIncomingMaterialRepository
{
    private readonly DasAppDbContext _db;
    public IncomingMaterialRepository(DasAppDbContext db) => _db = db;

    public Task<List<IncomingMaterial>> GetAllAsync()
        => _db.IncomingMaterials.OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<IncomingMaterial?> GetByIdAsync(int id)
        => _db.IncomingMaterials.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<int> InsertAsync(IncomingMaterial entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _db.IncomingMaterials.AddAsync(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(IncomingMaterial entity)
    {
        _db.IncomingMaterials.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.IncomingMaterials.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return false;
        _db.IncomingMaterials.Remove(entity);
        return await _db.SaveChangesAsync() > 0;
    }
}
