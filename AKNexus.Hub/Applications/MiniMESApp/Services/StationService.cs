using AKNexus.Framework.Application.Dtos;
using MiniMESApp.Dtos;
using MiniMESApp.Entities;
using MiniMESApp.Repositories;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Mvc;

namespace MiniMESApp.Services;

/// <summary>
/// 站点服务接口
/// </summary>
public interface IStationService
{
    /// <summary>
    /// 获取所有站点
    /// </summary>
    Task<Result<List<StationDto>>> GetAllAsync();

    /// <summary>
    /// 根据ID获取站点
    /// </summary>
    Task<Result<StationDto>> GetByIdAsync(int id);

    /// <summary>
    /// 创建站点
    /// </summary>
    Task<Result<StationDto>> CreateAsync(StationCreateDto dto);

    /// <summary>
    /// 更新站点
    /// </summary>
    Task<Result> UpdateAsync(int id, StationCreateDto dto);

    /// <summary>
    /// 删除站点
    /// </summary>
    Task<Result> DeleteAsync(int id);
}

/// <summary>
/// 站点服务 - 动态 API
/// </summary>
/// <remarks>
/// 使用 IDynamicApiController，无需创建 Controller
/// 自动生成路由：/api/station/get-all-async, /api/station/get-by-id-async 等
/// </remarks>
[ApiDescriptionSettings("Station", Name = "Station")]
public class StationService : IStationService, IDynamicApiController
{
    private readonly IStationRepository _repo;

    public StationService(IStationRepository repo) => _repo = repo;

    /// <summary>
    /// 获取所有站点
    /// </summary>
    /// <remarks>GET /api/station/get-all-async</remarks>
    [HttpGet("")]
    public async Task<Result<List<StationDto>>> GetAllAsync()
    {
        var stations = await _repo.GetAllAsync();
        var dtos = stations.Select(ToDto).ToList();
        return Result<List<StationDto>>.Ok(dtos);
    }

    /// <summary>
    /// 根据ID获取站点
    /// </summary>
    /// <remarks>GET /api/station/get-by-id-async?id={id}</remarks>
    [HttpGet("{id}")]
    public async Task<Result<StationDto>> GetByIdAsync(int id)
    {
        var s = await _repo.GetById(id);
        if (s == null) return Result<StationDto>.Fail("资源不存在");
        return Result<StationDto>.Ok(ToDto(s));
    }

    /// <summary>
    /// 创建站点
    /// </summary>
    /// <remarks>POST /api/station/create-async</remarks>
    [HttpPost("")]
    public async Task<Result<StationDto>> CreateAsync(StationCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.StationCode))
            return Result<StationDto>.Fail("站点编码不能为空");
        if (string.IsNullOrWhiteSpace(dto.StationName))
            return Result<StationDto>.Fail("站点名称不能为空");

        var existing = await _repo.GetByCode(dto.StationCode);
        if (existing != null)
            return Result<StationDto>.Fail($"站点编码 '{dto.StationCode}' 已存在");

        var station = new Station
        {
            StationCode = dto.StationCode.Trim(),
            StationName = dto.StationName.Trim(),
            StationType = dto.StationType,
            Location = dto.Location?.Trim(),
            SortOrder = dto.SortOrder,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _repo.InsertAsync(station);
        station.Id = id;
        return Result<StationDto>.Ok(ToDto(station), "创建成功");
    }

    /// <summary>
    /// 更新站点
    /// </summary>
    /// <remarks>PUT /api/station/update-async?id={id}</remarks>
    [HttpPut("{id}")]
    public async Task<Result> UpdateAsync(int id, StationCreateDto dto)
    {
        var station = await _repo.GetById(id);
        if (station == null) return Result.Fail("资源不存在");

        var existing = await _repo.GetByCode(dto.StationCode);
        if (existing != null && existing.Id != id)
            return Result.Fail($"站点编码 '{dto.StationCode}' 已被其他站点使用");

        station.StationCode = dto.StationCode.Trim();
        station.StationName = dto.StationName.Trim();
        station.StationType = dto.StationType;
        station.Location = dto.Location?.Trim();
        station.SortOrder = dto.SortOrder;

        await _repo.UpdateAsync(station);
        return Result.Ok("更新成功");
    }

    /// <summary>
    /// 删除站点
    /// </summary>
    /// <remarks>DELETE /api/station/delete-async?id={id}</remarks>
    [HttpDelete("{id}")]
    public async Task<Result> DeleteAsync(int id)
    {
        var station = await _repo.GetById(id);
        if (station == null) return Result.Fail("资源不存在");
        await _repo.DeleteAsync(id);
        return Result.Ok("删除成功");
    }

    private static StationDto ToDto(Station s) => new()
    {
        Id = s.Id,
        StationCode = s.StationCode,
        StationName = s.StationName,
        StationType = s.StationType,
        Location = s.Location,
        SortOrder = s.SortOrder
    };
}
