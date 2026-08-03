using MiniMESApp.Entities;

namespace MiniMESApp.Repositories;

public interface IStationRepository
{
    Task<Station?> GetByCode(string code);
    Task<Station?> GetById(int id);
    Task<List<Station>> GetByIdsAsync(List<long> ids);
    Task<List<Station>> GetAllAsync();
    Task<int> InsertAsync(Station station);
    Task UpdateAsync(Station station);
    Task DeleteAsync(int id);
}
