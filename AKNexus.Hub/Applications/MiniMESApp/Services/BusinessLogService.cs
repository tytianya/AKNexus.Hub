using AKNexus.Framework.Abstractions.Dtos;
using AKNexus.Framework.Application.Dtos;
using MiniMESApp.Dtos;
using MiniMESApp.Entities;
using MiniMESApp.Repositories;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Mvc;

namespace MiniMESApp.Services;

/// <summary>
/// 业务日志服务接口
/// </summary>
public interface IBusinessLogService
{
    /// <summary>
    /// 分页查询业务日志
    /// </summary>
    Task<Result<PagedResult<BusinessLogListDto>>> GetPagedAsync([FromQuery] BusinessLogQueryDto query);

    /// <summary>
    /// 推送业务日志
    /// </summary>
    Task<Result> PushAsync(BusinessLogPushInput input);
}

/// <summary>
/// 业务日志服务
/// </summary>
[ApiDescriptionSettings("BusinessLog", Name = "BusinessLog")]
public class BusinessLogService : IBusinessLogService, IDynamicApiController
{
    private readonly IBusinessLogRepository _repo;

    public BusinessLogService(IBusinessLogRepository repo)
    {
        _repo = repo;
    }

    /// <summary>
    /// 分页查询业务日志
    /// </summary>
    /// <remarks>GET /api/BusinessLog</remarks>
    [HttpGet("")]
    public async Task<Result<PagedResult<BusinessLogListDto>>> GetPagedAsync([FromQuery] BusinessLogQueryDto query)
    {
        var (items, total) = await _repo.GetPagedAsync(
            query.Type, query.Status, query.Keyword,
            query.PageNumber, query.PageSize);

        var dtos = items.Select(x => new BusinessLogListDto
        {
            Id = x.Id,
            Type = x.Type,
            TypeName = GetTypeName(x.Type),
            Code = x.Code,
            Content = x.Content,
            Remark = x.Remark,
            Status = x.Status,
            CreatedAt = x.CreatedAt
        }).ToList();

        return Result<PagedResult<BusinessLogListDto>>.Ok(new PagedResult<BusinessLogListDto>
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        });
    }

    /// <summary>
    /// 推送业务日志
    /// </summary>
    /// <remarks>POST /api/BusinessLog/push</remarks>
    [HttpPost("push")]
    public async Task<Result> PushAsync(BusinessLogPushInput input)
    {
        var entity = new BusinessLog
        {
            Type = input.Type,
            Code = GetCode(input.Type),
            Content = input.ResolveContent(),
            Remark = input.Remark,
            Status = input.Status,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.InsertAsync(entity);
        return Result.Ok("推送成功");
    }

    /// <summary>
    /// 业务类型 -> 业务编码映射
    /// </summary>
    private static string GetCode(BusinessType type) => type switch
    {
        BusinessType.BarcodeTest => "BarCode",
        BusinessType.StationTest => "Station",
        _ => "Unknown"
    };

    /// <summary>
    /// 业务类型 -> 中文名映射
    /// </summary>
    private static string GetTypeName(BusinessType type) => type switch
    {
        BusinessType.BarcodeTest => "条码测试",
        BusinessType.StationTest => "站点测试",
        _ => type.ToString()
    };
}
