using MiniMESApp.Entities;

namespace MiniMESApp.Repositories;

public interface IStationSoftwareBindingRepository
{
    Task<List<StationSoftwareBinding>> GetByStation(int stationId);
    Task<StationSoftwareBinding?> GetByStationAndSoftware(int stationId, int softwareId);
    Task<int> InsertAsync(StationSoftwareBinding binding);
    Task DeleteAsync(int bindingId);
    Task DeleteByStationAndSoftware(int stationId, int softwareId);
}
