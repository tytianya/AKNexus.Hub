using MiniMESApp.Entities;

namespace MiniMESApp.Repositories;

public interface IMaterialBarcodeRepository
{
    Task<List<MaterialBarcode>> GetByIncomingMaterialIdAsync(int incomingMaterialId);
    Task<int> GetCountByMaterialIdAsync(int incomingMaterialId);
    Task BulkInsertAsync(List<MaterialBarcode> barcodes);
    Task<MaterialBarcode?> GetByBarcode(string barcode);
    Task<bool> UpdateAsync(MaterialBarcode entity);
}
