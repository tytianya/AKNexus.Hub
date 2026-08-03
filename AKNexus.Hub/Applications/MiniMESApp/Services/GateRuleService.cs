using AKNexus.Framework.Application.Dtos;
using MiniMESApp.Dtos;
using MiniMESApp.Entities;
using MiniMESApp.Repositories;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Mvc;

namespace MiniMESApp.Services;

/// <summary>
/// 卡关规则服务接口
/// </summary>
public interface IGateRuleService
{
    /// <summary>
    /// 获取所有卡关规则
    /// </summary>
    Task<Result<List<GateRuleDto>>> GetAllAsync();

    /// <summary>
    /// 创建卡关规则
    /// </summary>
    Task<Result<GateRuleDto>> CreateAsync(GateRuleDto dto);

    /// <summary>
    /// 更新卡关规则
    /// </summary>
    Task<Result> UpdateAsync(int id, GateRuleDto dto);

    /// <summary>
    /// 删除卡关规则
    /// </summary>
    Task<Result> DeleteAsync(int id);
}

/// <summary>
/// 卡关规则服务 - 动态 API
/// </summary>
/// <remarks>
/// 使用 IDynamicApiController，无需创建 Controller
/// 自动生成路由：/api/gate-rule/get-all-async, /api/gate-rule/create-async 等
/// </remarks>
[ApiDescriptionSettings("GateRule", Name = "GateRule")]
public class GateRuleService : IGateRuleService, IDynamicApiController
{
    private readonly IGateRuleRepository _repo;
    private readonly IStationRepository _stationRepo;

    public GateRuleService(IGateRuleRepository repo, IStationRepository stationRepo)
    {
        _repo = repo;
        _stationRepo = stationRepo;
    }

    /// <summary>
    /// 获取所有卡关规则
    /// </summary>
    /// <remarks>GET /api/gate-rule/get-all-async</remarks>
    [HttpGet("")]
    public async Task<Result<List<GateRuleDto>>> GetAllAsync()
    {
        var rules = await _repo.GetAllAsync();
        var stations = await _stationRepo.GetAllAsync();
        var stationMap = stations.ToDictionary(s => s.Id);

        var dtos = rules.Select(r => new GateRuleDto
        {
            Id = r.Id, StationId = r.StationId,
            StationCode = stationMap.GetValueOrDefault(r.StationId)?.StationCode ?? "",
            StationName = stationMap.GetValueOrDefault(r.StationId)?.StationName ?? "",
            RuleName = r.RuleName, RuleType = r.RuleType,
            Condition = r.Condition, IsActive = r.IsActive, Priority = r.Priority
        }).ToList();
        return Result<List<GateRuleDto>>.Ok(dtos);
    }

    /// <summary>
    /// 创建卡关规则
    /// </summary>
    /// <remarks>POST /api/gate-rule/create-async</remarks>
    [HttpPost("")]
    public async Task<Result<GateRuleDto>> CreateAsync(GateRuleDto dto)
    {
        var rule = new GateRule
        {
            StationId = dto.StationId,
            RuleName = dto.RuleName,
            RuleType = dto.RuleType,
            Condition = dto.Condition,
            IsActive = dto.IsActive,
            Priority = dto.Priority,
            CreatedAt = DateTime.UtcNow
        };
        rule.Id = await _repo.InsertAsync(rule);
        dto.Id = rule.Id;
        return Result<GateRuleDto>.Ok(dto, "创建成功");
    }

    /// <summary>
    /// 更新卡关规则
    /// </summary>
    /// <remarks>PUT /api/gate-rule/update-async?id={id}</remarks>
    [HttpPut("{id}")]
    public async Task<Result> UpdateAsync(int id, GateRuleDto dto)
    {
        var rule = await _repo.GetById(id);
        if (rule == null) return Result.Fail("资源不存在");
        rule.RuleName = dto.RuleName;
        rule.RuleType = dto.RuleType;
        rule.Condition = dto.Condition;
        rule.IsActive = dto.IsActive;
        rule.Priority = dto.Priority;
        await _repo.UpdateAsync(rule);
        return Result.Ok("更新成功");
    }

    /// <summary>
    /// 删除卡关规则
    /// </summary>
    /// <remarks>DELETE /api/gate-rule/delete-async?id={id}</remarks>
    [HttpDelete("{id}")]
    public async Task<Result> DeleteAsync(int id)
    {
        await _repo.DeleteAsync(id);
        return Result.Ok("删除成功");
    }
}
