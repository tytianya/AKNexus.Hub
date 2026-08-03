using AKNexus.Framework.Application.Dtos;
using MiniMESApp.Dtos;
using MiniMESApp.Repositories;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Mvc;

namespace MiniMESApp.Services;

/// <summary>
/// 过站记录查询服务接口
/// </summary>
public interface IQueryService
{
    /// <summary>
    /// 分页查询过站记录
    /// </summary>
    Task<Result<PagedResult<ProductStationLogDto>>> QueryLogsAsync(ProductQueryRequest request);
}

/// <summary>
/// 过站记录查询服务 - 动态 API
/// </summary>
[ApiDescriptionSettings("Query", Name = "Query")]
public class QueryService : IQueryService, IDynamicApiController
{
    private readonly IProductStationLogRepository _logRepo;

    public QueryService(IProductStationLogRepository logRepo) => _logRepo = logRepo;

    /// <summary>
    /// 分页查询过站记录
    /// </summary>
    /// <remarks>POST /api/Query/logs</remarks>
    [HttpPost("logs")]
    public async Task<Result<PagedResult<ProductStationLogDto>>> QueryLogsAsync(ProductQueryRequest request)
    {
        var result = await _logRepo.QueryAsync(request);
        return Result<PagedResult<ProductStationLogDto>>.Ok(result);
    }
}
