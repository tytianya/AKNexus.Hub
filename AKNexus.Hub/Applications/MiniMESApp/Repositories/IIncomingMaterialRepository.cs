using MiniMESApp.Entities;

namespace MiniMESApp.Repositories;

public interface IIncomingMaterialRepository
{
    Task<List<IncomingMaterial>> GetAllAsync();
    Task<IncomingMaterial?> GetByIdAsync(int id);
    Task<int> InsertAsync(IncomingMaterial entity);
    Task<bool> UpdateAsync(IncomingMaterial entity);
    Task<bool> DeleteAsync(int id);
}
