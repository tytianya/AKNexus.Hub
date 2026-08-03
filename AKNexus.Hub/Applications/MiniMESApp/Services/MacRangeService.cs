using AKNexus.Framework.Application.Dtos;
using MiniMESApp.Dtos;
using MiniMESApp.Entities;
using MiniMESApp.Repositories;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Mvc;

namespace MiniMESApp.Services;

/// <summary>
/// MAC 地址段管理服务接口
/// </summary>
public interface IMacRangeService
{
    /// <summary>
    /// 获取所有 MAC 地址段
    /// </summary>
    Task<Result<List<MacRangeDto>>> GetAllAsync();

    /// <summary>
    /// 创建 MAC 地址段
    /// </summary>
    Task<Result<MacRangeDto>> CreateAsync(MacRangeCreateDto dto);

    /// <summary>
    /// 更新 MAC 地址段
    /// </summary>
    Task<Result> UpdateAsync(int id, MacRangeCreateDto dto);

    /// <summary>
    /// 删除 MAC 地址段
    /// </summary>
    Task<Result> DeleteAsync(int id);
}

/// <summary>
/// MAC 地址段管理服务 - 动态 API
/// </summary>
[ApiDescriptionSettings("MacRange", Name = "MacRange")]
public class MacRangeService : IMacRangeService, IDynamicApiController
{
    private readonly IMacRangeRepository _repo;
    private readonly IMacAllocationRepository _allocRepo;

    public MacRangeService(IMacRangeRepository repo, IMacAllocationRepository allocRepo)
    {
        _repo = repo;
        _allocRepo = allocRepo;
    }

    /// <summary>
    /// 获取所有 MAC 地址段
    /// </summary>
    /// <remarks>GET /api/MacRange</remarks>
    [HttpGet("")]
    public async Task<Result<List<MacRangeDto>>> GetAllAsync()
    {
        var ranges = await _repo.GetAllAsync();
        var allocations = await _allocRepo.GetAllAsync();

        var dtos = ranges.Select(r =>
        {
            var start = MacAllocationService.MacToLong(r.StartMac);
            var end = MacAllocationService.MacToLong(r.EndMac);
            var total = (int)(end - start + 1);
            var allocCount = allocations.Count(a => a.MacRangeId == r.Id);
            return new MacRangeDto
            {
                Id = r.Id, RangeName = r.RangeName, StartMac = r.StartMac, EndMac = r.EndMac,
                NextMac = r.NextMac, TotalCount = total > 0 ? total : 0,
                AllocatedCount = allocCount, RemainingCount = total > 0 ? total - allocCount : 0,
                IsActive = r.IsActive, CreatedAt = r.CreatedAt
            };
        }).ToList();
        return Result<List<MacRangeDto>>.Ok(dtos);
    }

    /// <summary>
    /// 创建 MAC 地址段
    /// </summary>
    /// <remarks>POST /api/MacRange</remarks>
    [HttpPost("")]
    public async Task<Result<MacRangeDto>> CreateAsync(MacRangeCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RangeName) || string.IsNullOrWhiteSpace(dto.StartMac) || string.IsNullOrWhiteSpace(dto.EndMac))
            return Result<MacRangeDto>.Fail("名称和起止 MAC 不能为空");

        var start = MacAllocationService.MacToLong(dto.StartMac);
        var end = MacAllocationService.MacToLong(dto.EndMac);
        if (start < 0 || end < 0 || start > end)
            return Result<MacRangeDto>.Fail("MAC 地址格式错误或起始大于结束");

        var range = new MacRange
        {
            RangeName = dto.RangeName.Trim(),
            StartMac = dto.StartMac.Trim().ToUpper(),
            EndMac = dto.EndMac.Trim().ToUpper(),
            NextMac = dto.StartMac.Trim().ToUpper(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        range.Id = await _repo.InsertAsync(range);
        return Result<MacRangeDto>.Ok(new MacRangeDto
        {
            Id = range.Id, RangeName = range.RangeName, StartMac = range.StartMac,
            EndMac = range.EndMac, NextMac = range.NextMac, TotalCount = (int)(end - start + 1),
            IsActive = range.IsActive, CreatedAt = range.CreatedAt
        }, "创建成功");
    }

    /// <summary>
    /// 更新 MAC 地址段
    /// </summary>
    /// <remarks>PUT /api/MacRange/{id}</remarks>
    [HttpPut("{id}")]
    public async Task<Result> UpdateAsync(int id, MacRangeCreateDto dto)
    {
        var range = await _repo.GetById(id);
        if (range == null) return Result.Fail("资源不存在");
        range.RangeName = dto.RangeName.Trim();
        await _repo.UpdateAsync(range);
        return Result.Ok("更新成功");
    }

    /// <summary>
    /// 删除 MAC 地址段
    /// </summary>
    /// <remarks>DELETE /api/MacRange/{id}</remarks>
    [HttpDelete("{id}")]
    public async Task<Result> DeleteAsync(int id)
    {
        var range = await _repo.GetById(id);
        if (range == null) return Result.Fail("资源不存在");
        await _repo.DeleteAsync(id);
        return Result.Ok("删除成功");
    }
}
