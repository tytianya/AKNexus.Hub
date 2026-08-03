using MiniMESApp.Entities;

namespace MiniMESApp.Repositories;

public interface IGateRuleRepository
{
    Task<List<GateRule>> GetActiveByStation(int stationId);
    Task<List<GateRule>> GetAllAsync();
    Task<GateRule?> GetById(int id);
    Task<int> InsertAsync(GateRule rule);
    Task UpdateAsync(GateRule rule);
    Task DeleteAsync(int id);
}
