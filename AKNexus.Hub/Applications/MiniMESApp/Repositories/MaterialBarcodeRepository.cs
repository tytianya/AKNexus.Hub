using MiniMESApp.Data;
using MiniMESApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniMESApp.Repositories;

public class MaterialBarcodeRepository : IMaterialBarcodeRepository
{
    private readonly DasAppDbContext _db;
    public MaterialBarcodeRepository(DasAppDbContext db) => _db = db;

    public Task<List<MaterialBarcode>> GetByIncomingMaterialIdAsync(int incomingMaterialId)
        => _db.MaterialBarcodes
            .Where(x => x.IncomingMaterialId == incomingMaterialId)
            .OrderBy(x => x.SequenceNo)
            .ToListAsync();

    public Task<int> GetCountByMaterialIdAsync(int incomingMaterialId)
        => _db.MaterialBarcodes
            .Where(x => x.IncomingMaterialId == incomingMaterialId)
            .CountAsync();

    public async Task BulkInsertAsync(List<MaterialBarcode> barcodes)
    {
        foreach (var b in barcodes)
        {
            b.CreatedAt = DateTime.UtcNow;
        }
        await _db.MaterialBarcodes.AddRangeAsync(barcodes);
        await _db.SaveChangesAsync();
    }

    public Task<MaterialBarcode?> GetByBarcode(string barcode)
        => _db.MaterialBarcodes.FirstOrDefaultAsync(b => b.Barcode == barcode);

    public async Task<bool> UpdateAsync(MaterialBarcode entity)
    {
        _db.MaterialBarcodes.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }
}
