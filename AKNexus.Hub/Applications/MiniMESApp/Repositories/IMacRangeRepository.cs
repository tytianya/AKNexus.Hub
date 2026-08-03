using MiniMESApp.Entities;

namespace MiniMESApp.Repositories;

public interface IMacRangeRepository
{
    Task<List<MacRange>> GetAllAsync();
    Task<MacRange?> GetById(int id);
    Task<int> InsertAsync(MacRange range);
    Task UpdateAsync(MacRange range);
    Task DeleteAsync(int id);
}
