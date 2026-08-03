using AKNexus.Framework.Application.Dtos;
using MiniMESApp.Dtos;
using MiniMESApp.Repositories;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Mvc;

namespace MiniMESApp.Services;

/// <summary>
/// MAC 分配服务接口
/// </summary>
public interface IMacService
{
    /// <summary>
    /// 分配 MAC 地址
    /// </summary>
    Task<Result<MacAllocationResult>> AllocateAsync(MacAllocateRequest request);

    /// <summary>
    /// 获取所有 MAC 分配记录
    /// </summary>
    Task<Result<List<MacAllocationDto>>> GetAllocationsAsync();
}

/// <summary>
/// MAC 分配服务 - 动态 API
/// </summary>
[ApiDescriptionSettings("Mac", Name = "Mac")]
public class MacService : IMacService, IDynamicApiController
{
    private readonly MacAllocationService _service;
    private readonly IMacRangeRepository _rangeRepo;
    private readonly IMacAllocationRepository _allocRepo;

    public MacService(MacAllocationService service, IMacRangeRepository rangeRepo, IMacAllocationRepository allocRepo)
    {
        _service = service;
        _rangeRepo = rangeRepo;
        _allocRepo = allocRepo;
    }

    /// <summary>
    /// 分配 MAC 地址
    /// </summary>
    /// <remarks>POST /api/Mac/allocate</remarks>
    [HttpPost("allocate")]
    public async Task<Result<MacAllocationResult>> AllocateAsync(MacAllocateRequest request)
    {
        var result = await _service.AllocateAsync(request.AllocatedTo);
        if (!result.Success)
            return Result<MacAllocationResult>.Fail(result.Message);
        return Result<MacAllocationResult>.Ok(result, "分配成功");
    }

    /// <summary>
    /// 获取所有 MAC 分配记录
    /// </summary>
    /// <remarks>GET /api/Mac/allocations</remarks>
    [HttpGet("allocations")]
    public async Task<Result<List<MacAllocationDto>>> GetAllocationsAsync()
    {
        var ranges = await _rangeRepo.GetAllAsync();
        var allocs = await _allocRepo.GetAllAsync();

        var dtos = allocs.Select(a =>
        {
            var range = ranges.FirstOrDefault(r => r.Id == a.MacRangeId);
            return new MacAllocationDto
            {
                Id = a.Id, MacAddress = a.MacAddress,
                RangeName = range?.RangeName ?? "-",
                AllocatedTo = a.AllocatedTo, AllocatedAt = a.AllocatedAt
            };
        }).ToList();
        return Result<List<MacAllocationDto>>.Ok(dtos);
    }
}
