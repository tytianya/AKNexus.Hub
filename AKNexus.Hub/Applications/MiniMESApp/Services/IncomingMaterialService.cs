using System.Text.Json;
using AKNexus.Framework.Application.Dtos;
using MiniMESApp.Dtos;
using MiniMESApp.Entities;
using MiniMESApp.Repositories;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Mvc;

namespace MiniMESApp.Services;

/// <summary>
/// 来料管理服务接口
/// </summary>
public interface IIncomingMaterialService
{
    /// <summary>
    /// 获取所有来料记录
    /// </summary>
    Task<Result<List<IncomingMaterialListDto>>> GetAllAsync();

    /// <summary>
    /// 根据ID获取来料详情
    /// </summary>
    Task<Result<object>> GetByIdAsync(int id);

    /// <summary>
    /// 获取来料条码列表
    /// </summary>
    Task<Result<List<MaterialBarcodeDto>>> GetBarcodesAsync(int id);

    /// <summary>
    /// 创建来料记录
    /// </summary>
    Task<Result<object>> CreateAsync(IncomingMaterialCreateDto dto);

    /// <summary>
    /// 更新来料记录
    /// </summary>
    Task<Result> UpdateAsync(int id, IncomingMaterialUpdateDto dto);

    /// <summary>
    /// 删除来料记录
    /// </summary>
    Task<Result> DeleteAsync(int id);
}

/// <summary>
/// 来料管理服务 - 动态 API
/// </summary>
[ApiDescriptionSettings("IncomingMaterial", Name = "IncomingMaterial")]
public class IncomingMaterialService : IIncomingMaterialService, IDynamicApiController
{
    private readonly IIncomingMaterialRepository _materialRepo;
    private readonly IMaterialBarcodeRepository _barcodeRepo;
    private readonly IStationFlowRepository _flowRepo;
    private readonly IStationRepository _stationRepo;
    private readonly IProductRepository _productRepo;
    private readonly IProductStationLogRepository _logRepo;

    public IncomingMaterialService(
        IIncomingMaterialRepository materialRepo,
        IMaterialBarcodeRepository barcodeRepo,
        IStationFlowRepository flowRepo,
        IStationRepository stationRepo,
        IProductRepository productRepo,
        IProductStationLogRepository logRepo)
    {
        _materialRepo = materialRepo;
        _barcodeRepo = barcodeRepo;
        _flowRepo = flowRepo;
        _stationRepo = stationRepo;
        _productRepo = productRepo;
        _logRepo = logRepo;
    }

    /// <summary>
    /// 获取所有来料记录
    /// </summary>
    /// <remarks>GET /api/IncomingMaterial</remarks>
    [HttpGet("")]
    public async Task<Result<List<IncomingMaterialListDto>>> GetAllAsync()
    {
        var list = await _materialRepo.GetAllAsync();
        var dtos = new List<IncomingMaterialListDto>();

        foreach (var m in list)
        {
            var count = await _barcodeRepo.GetCountByMaterialIdAsync(m.Id);
            dtos.Add(new IncomingMaterialListDto
            {
                Id = m.Id, MaterialCode = m.MaterialCode, ProductModel = m.ProductModel,
                Quantity = m.Quantity, BarcodeCount = count, Supplier = m.Supplier,
                LotNo = m.LotNo, InspectResult = m.InspectResult, InspectedBy = m.InspectedBy,
                InspectedAt = m.InspectedAt, Remark = m.Remark, CreatedAt = m.CreatedAt
            });
        }
        return Result<List<IncomingMaterialListDto>>.Ok(dtos);
    }

    /// <summary>
    /// 根据ID获取来料详情
    /// </summary>
    /// <remarks>GET /api/IncomingMaterial/{id}</remarks>
    [HttpGet("{id}")]
    public async Task<Result<object>> GetByIdAsync(int id)
    {
        var m = await _materialRepo.GetByIdAsync(id);
        if (m == null) return Result<object>.Fail("资源不存在");

        var barcodes = await _barcodeRepo.GetByIncomingMaterialIdAsync(id);
        return Result<object>.Ok(new
        {
            material = m,
            barcodes = barcodes.Select(b => new MaterialBarcodeDto
            {
                Id = b.Id, IncomingMaterialId = b.IncomingMaterialId, SequenceNo = b.SequenceNo,
                Barcode = b.Barcode, CurrentStationId = b.CurrentStationId, IsUsed = b.IsUsed,
                UsedAt = b.UsedAt, CreatedAt = b.CreatedAt
            })
        });
    }

    /// <summary>
    /// 获取来料条码列表
    /// </summary>
    /// <remarks>GET /api/IncomingMaterial/{id}/barcodes</remarks>
    [HttpGet("{id}/barcodes")]
    public async Task<Result<List<MaterialBarcodeDto>>> GetBarcodesAsync(int id)
    {
        var barcodes = await _barcodeRepo.GetByIncomingMaterialIdAsync(id);
        var stationIds = barcodes.Where(b => b.CurrentStationId.HasValue)
            .Select(b => b.CurrentStationId!.Value).Distinct().ToList();
        var stations = await _stationRepo.GetByIdsAsync(stationIds);
        var stationMap = stations.ToDictionary(s => (long)s.Id, s => s.StationName);

        var dtos = barcodes.Select(b => new MaterialBarcodeDto
        {
            Id = b.Id, IncomingMaterialId = b.IncomingMaterialId, SequenceNo = b.SequenceNo,
            Barcode = b.Barcode, CurrentStationId = b.CurrentStationId,
            StationName = b.CurrentStationId.HasValue && stationMap.TryGetValue(b.CurrentStationId.Value, out var name) ? name : null,
            IsUsed = b.IsUsed, UsedAt = b.UsedAt, CreatedAt = b.CreatedAt
        }).ToList();
        return Result<List<MaterialBarcodeDto>>.Ok(dtos);
    }

    /// <summary>
    /// 创建来料记录
    /// </summary>
    /// <remarks>POST /api/IncomingMaterial</remarks>
    [HttpPost("")]
    public async Task<Result<object>> CreateAsync(IncomingMaterialCreateDto dto)
    {
        if (dto.Quantity <= 0 || dto.Quantity > 9999)
            return Result<object>.Fail("来料数量必须在 1~9999 之间");
        if (string.IsNullOrWhiteSpace(dto.ProductModel))
            return Result<object>.Fail("产品型号不能为空");

        var material = new IncomingMaterial
        {
            MaterialCode = dto.MaterialCode,
            ProductModel = dto.ProductModel.Trim(),
            Quantity = dto.Quantity,
            FlowId = dto.FlowId,
            Supplier = dto.Supplier,
            LotNo = dto.LotNo,
            InspectResult = "Pending",
            Remark = dto.Remark,
            CreatedAt = DateTime.UtcNow
        };
        var materialId = await _materialRepo.InsertAsync(material);

        long? nextStationId = null;
        if (dto.FlowId.HasValue)
        {
            nextStationId = await FindNextStationAsync(dto.FlowId.Value);
        }

        var barcodeValues = BarcodeGenerator.Generate(material.ProductModel, dto.Quantity);
        var barcodes = barcodeValues.Select((val, idx) => new MaterialBarcode
        {
            IncomingMaterialId = materialId,
            SequenceNo = idx + 1,
            Barcode = val,
            CurrentStationId = nextStationId,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await _barcodeRepo.BulkInsertAsync(barcodes);

        int logCount = 0;
        if (dto.FlowId.HasValue && nextStationId.HasValue)
        {
            var product = await _productRepo.GetByCode(material.ProductModel);
            if (product != null)
            {
                foreach (var barcode in barcodes)
                {
                    await _logRepo.InsertAsync(new ProductStationLog
                    {
                        ProductId = product.Id,
                        StationId = (int)nextStationId.Value,
                        FlowId = dto.FlowId.Value,
                        PassResult = "Pass",
                        BarcodeId = barcode.Id,
                        Remark = $"来料检测[{material.MaterialCode}]条码分配",
                        Operator = "System",
                        PassTime = DateTime.UtcNow
                    });
                    logCount++;
                }
            }
        }

        return Result<object>.Ok(new
        {
            materialId, barcodeCount = barcodes.Count, logCount,
            firstBarcode = barcodes.FirstOrDefault()?.Barcode,
            lastBarcode = barcodes.LastOrDefault()?.Barcode,
            message = $"成功生成 {dto.Quantity} 个条码，写入 {logCount} 条过站记录"
        }, "创建成功");
    }

    /// <summary>
    /// 更新来料记录
    /// </summary>
    /// <remarks>PUT /api/IncomingMaterial/{id}</remarks>
    [HttpPut("{id}")]
    public async Task<Result> UpdateAsync(int id, IncomingMaterialUpdateDto dto)
    {
        var m = await _materialRepo.GetByIdAsync(id);
        if (m == null) return Result.Fail("资源不存在");

        m.InspectResult = dto.InspectResult;
        m.InspectedBy = dto.InspectedBy;
        m.InspectedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(dto.Remark))
            m.Remark = dto.Remark;

        await _materialRepo.UpdateAsync(m);
        return Result.Ok("更新成功");
    }

    /// <summary>
    /// 删除来料记录
    /// </summary>
    /// <remarks>DELETE /api/IncomingMaterial/{id}</remarks>
    [HttpDelete("{id}")]
    public async Task<Result> DeleteAsync(int id)
    {
        var m = await _materialRepo.GetByIdAsync(id);
        if (m == null) return Result.Fail("资源不存在");
        await _materialRepo.DeleteAsync(id);
        return Result.Ok("删除成功");
    }

    private async Task<long?> FindNextStationAsync(int flowId)
    {
        var flow = await _flowRepo.GetById(flowId);
        if (flow == null) return null;

        try
        {
            var steps = JsonSerializer.Deserialize<List<FlowStep>>(flow.FlowJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (steps == null || steps.Count == 0) return null;
            return steps.OrderBy(s => s.Order).First().StationId;
        }
        catch
        {
            return null;
        }
    }

    private class FlowStep
    {
        public int StationId { get; set; }
        public int Order { get; set; }
        public bool Required { get; set; }
    }
}
