using System.Text.Json;
using AKNexus.Framework.Application.Dtos;
using MiniMESApp.Dtos;
using MiniMESApp.Entities;
using MiniMESApp.Repositories;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Mvc;

namespace MiniMESApp.Services;

/// <summary>
/// 站点流程服务接口
/// </summary>
public interface IStationFlowService
{
    /// <summary>
    /// 获取所有流程
    /// </summary>
    Task<Result<List<StationFlowDto>>> GetAllAsync();

    /// <summary>
    /// 根据ID获取流程
    /// </summary>
    Task<Result<StationFlowDto>> GetByIdAsync(int id);

    /// <summary>
    /// 创建流程
    /// </summary>
    Task<Result<StationFlowDto>> CreateAsync(StationFlowDto dto);

    /// <summary>
    /// 更新流程
    /// </summary>
    Task<Result> UpdateAsync(int id, StationFlowDto dto);

    /// <summary>
    /// 删除流程
    /// </summary>
    Task<Result> DeleteAsync(int id);
}

/// <summary>
/// 站点流程服务 - 动态 API
/// </summary>
/// <remarks>
/// 使用 IDynamicApiController，无需创建 Controller
/// 自动生成路由：/api/station-flow/get-all-async, /api/station-flow/get-by-id-async 等
/// </remarks>
[ApiDescriptionSettings("StationFlow", Name = "StationFlow")]
public class StationFlowService : IStationFlowService, IDynamicApiController
{
    private readonly IStationFlowRepository _flowRepo;
    private readonly IStationRepository _stationRepo;

    public StationFlowService(IStationFlowRepository flowRepo, IStationRepository stationRepo)
    {
        _flowRepo = flowRepo;
        _stationRepo = stationRepo;
    }

    /// <summary>
    /// 获取所有流程
    /// </summary>
    /// <remarks>GET /api/station-flow/get-all-async</remarks>
    [HttpGet("")]
    public async Task<Result<List<StationFlowDto>>> GetAllAsync()
    {
        var flows = await _flowRepo.GetAllAsync();
        var stations = await _stationRepo.GetAllAsync();
        var stationMap = stations.ToDictionary(s => s.Id);

        var dtos = flows.Select(f =>
        {
            var flowStations = JsonSerializer.Deserialize<List<FlowStationItem>>(f.FlowJson)
                ?? new List<FlowStationItem>();
            foreach (var fs in flowStations)
            {
                if (stationMap.TryGetValue(fs.StationId, out var st))
                {
                    fs.StationCode = st.StationCode;
                    fs.StationName = st.StationName;
                }
            }
            return new StationFlowDto
            {
                Id = f.Id, FlowCode = f.FlowCode, FlowName = f.FlowName,
                Description = f.Description ?? "", Stations = flowStations,
                IsActive = f.IsActive, CreatedAt = f.CreatedAt, UpdatedAt = f.UpdatedAt
            };
        }).ToList();
        return Result<List<StationFlowDto>>.Ok(dtos);
    }

    /// <summary>
    /// 根据ID获取流程
    /// </summary>
    /// <remarks>GET /api/station-flow/get-by-id-async?id={id}</remarks>
    [HttpGet("{id}")]
    public async Task<Result<StationFlowDto>> GetByIdAsync(int id)
    {
        var f = await _flowRepo.GetById(id);
        if (f == null) return Result<StationFlowDto>.Fail("资源不存在");

        var stations = await _stationRepo.GetAllAsync();
        var stationMap = stations.ToDictionary(s => s.Id);
        var flowStations = JsonSerializer.Deserialize<List<FlowStationItem>>(f.FlowJson)
            ?? new List<FlowStationItem>();
        foreach (var fs in flowStations)
        {
            if (stationMap.TryGetValue(fs.StationId, out var st))
            {
                fs.StationCode = st.StationCode;
                fs.StationName = st.StationName;
            }
        }
        return Result<StationFlowDto>.Ok(new StationFlowDto
        {
            Id = f.Id, FlowCode = f.FlowCode, FlowName = f.FlowName,
            Description = f.Description ?? "", Stations = flowStations,
            IsActive = f.IsActive, CreatedAt = f.CreatedAt, UpdatedAt = f.UpdatedAt
        });
    }

    /// <summary>
    /// 创建流程
    /// </summary>
    /// <remarks>POST /api/station-flow/create-async</remarks>
    [HttpPost("")]
    public async Task<Result<StationFlowDto>> CreateAsync(StationFlowDto dto)
    {
        var flow = new StationFlow
        {
            FlowCode = dto.FlowCode,
            FlowName = dto.FlowName,
            Description = dto.Description,
            FlowJson = JsonSerializer.Serialize(dto.Stations.Select(s => new
            {
                stationId = s.StationId,
                order = s.Order,
                required = s.Required
            })),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        flow.Id = await _flowRepo.InsertAsync(flow);
        dto.Id = flow.Id;
        dto.CreatedAt = flow.CreatedAt;
        return Result<StationFlowDto>.Ok(dto, "创建成功");
    }

    /// <summary>
    /// 更新流程
    /// </summary>
    /// <remarks>PUT /api/station-flow/update-async?id={id}</remarks>
    [HttpPut("{id}")]
    public async Task<Result> UpdateAsync(int id, StationFlowDto dto)
    {
        var flow = await _flowRepo.GetById(id);
        if (flow == null) return Result.Fail("资源不存在");

        flow.FlowName = dto.FlowName;
        flow.Description = dto.Description;
        flow.FlowJson = JsonSerializer.Serialize(dto.Stations.Select(s => new
        {
            stationId = s.StationId,
            order = s.Order,
            required = s.Required
        }));
        flow.IsActive = dto.IsActive;
        flow.UpdatedAt = DateTime.UtcNow;

        await _flowRepo.UpdateAsync(flow);
        return Result.Ok("更新成功");
    }

    /// <summary>
    /// 删除流程
    /// </summary>
    /// <remarks>DELETE /api/station-flow/delete-async?id={id}</remarks>
    [HttpDelete("{id}")]
    public async Task<Result> DeleteAsync(int id)
    {
        await _flowRepo.DeleteAsync(id);
        return Result.Ok("删除成功");
    }
}
