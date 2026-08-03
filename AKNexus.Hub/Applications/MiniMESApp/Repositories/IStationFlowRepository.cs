using MiniMESApp.Entities;

namespace MiniMESApp.Repositories;

public interface IStationFlowRepository
{
    Task<List<StationFlow>> GetAllAsync();
    Task<StationFlow?> GetById(int id);
    Task<StationFlow?> GetByCode(string code);
    Task<int> InsertAsync(StationFlow flow);
    Task UpdateAsync(StationFlow flow);
    Task DeleteAsync(int id);
}
