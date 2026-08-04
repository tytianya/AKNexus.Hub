using AKNexus.Framework.Abstractions.Dtos;
using MiniMESApp.Dtos;
using MiniMESApp.Entities;

namespace MiniMESApp.Repositories;

public interface IProductStationLogRepository
{
    Task<long> InsertAsync(ProductStationLog log);
    Task<ProductStationLog?> GetLastLog(int productId, int flowId);
    Task<PagedResult<ProductStationLogDto>> QueryAsync(ProductQueryRequest req);
}
