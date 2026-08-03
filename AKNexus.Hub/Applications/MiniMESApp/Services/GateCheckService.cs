using AKNexus.Framework.Application.Dtos;
using MiniMESApp.Dtos;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Mvc;

namespace MiniMESApp.Services;

/// <summary>
/// 过站检查服务接口
/// </summary>
public interface IGateCheckService
{
    /// <summary>
    /// 过站检查
    /// </summary>
    Task<Result<GateCheckResult>> CheckAsync(GateCheckRequest request);
}

/// <summary>
/// 过站检查服务 - 动态 API
/// </summary>
/// <remarks>
/// 使用 IDynamicApiController，无需创建 Controller
/// 自动生成路由：/api/gate-check/check-async
/// 本服务为 API 层，仅负责委托调用 GateCheckProvider 进行业务处理
/// </remarks>
[ApiDescriptionSettings("GateCheck", Name = "GateCheck")]
public class GateCheckService : IGateCheckService, IDynamicApiController
{
    private readonly GateCheckProvider _provider;

    public GateCheckService(GateCheckProvider provider) => _provider = provider;

    /// <summary>
    /// 过站检查
    /// </summary>
    /// <remarks>POST /api/gate-check/check-async</remarks>
    [HttpPost("check")]
    public async Task<Result<GateCheckResult>> CheckAsync(GateCheckRequest request)
    {
        var result = await _provider.CheckAsync(request);
        return Result<GateCheckResult>.Ok(result, result.Message);
    }
}
