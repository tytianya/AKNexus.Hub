using System.Text.Json;
using MiniMESApp.Dtos;
using MiniMESApp.Entities;
using MiniMESApp.Repositories;

namespace MiniMESApp.Services;

public class GateCheckProvider
{
    private readonly IProductRepository _productRepo;
    private readonly IStationRepository _stationRepo;
    private readonly IStationFlowRepository _flowRepo;
    private readonly IProductFlowBindingRepository _bindingRepo;
    private readonly IGateRuleRepository _ruleRepo;
    private readonly IMaterialBarcodeRepository _barcodeRepo;
    private readonly IIncomingMaterialRepository _incomingMaterialRepo;
    private readonly ILogger<GateCheckProvider> _logger;

    public GateCheckProvider(
        IProductRepository productRepo,
        IStationRepository stationRepo,
        IStationFlowRepository flowRepo,
        IProductFlowBindingRepository bindingRepo,
        IGateRuleRepository ruleRepo,
        IMaterialBarcodeRepository barcodeRepo,
        IIncomingMaterialRepository incomingMaterialRepo,
        ILogger<GateCheckProvider> logger)
    {
        _productRepo = productRepo;
        _stationRepo = stationRepo;
        _flowRepo = flowRepo;
        _bindingRepo = bindingRepo;
        _ruleRepo = ruleRepo;
        _barcodeRepo = barcodeRepo;
        _incomingMaterialRepo = incomingMaterialRepo;
        _logger = logger;
    }

    public async Task<GateCheckResult> CheckAsync(GateCheckRequest request)
    {
        Product? product;
        MaterialBarcode? barcode = null;
        StationFlow? flow = null;
        bool isBarcodeMode = false;

        product = await _productRepo.GetByCode(request.ProductCode);

        if (product == null)
        {
            barcode = await _barcodeRepo.GetByBarcode(request.ProductCode);
            if (barcode == null)
                return Fail($"产品/条码 [{request.ProductCode}] 不存在");

            if (barcode.IsUsed)
                return Fail($"条码 [{request.ProductCode}] 已被使用（于 {barcode.UsedAt:yyyy-MM-dd HH:mm:ss}）");

            isBarcodeMode = true;

            var incoming = await _incomingMaterialRepo.GetByIdAsync(barcode.IncomingMaterialId);
            if (incoming == null)
                return Fail($"条码 [{request.ProductCode}] 关联的来料记录不存在");

            product = await _productRepo.GetByCode(incoming.ProductModel);
            if (product == null)
                return Fail($"产品型号 [{incoming.ProductModel}] 在系统中不存在，请先在产品管理中添加");

            if (incoming.FlowId.HasValue)
            {
                flow = await _flowRepo.GetById(incoming.FlowId.Value);
                if (flow == null || !flow.IsActive)
                    return Fail($"条码关联的流程 [{incoming.FlowId}] 无效或已停用");
            }
        }

        if (product == null)
            return Fail($"内部错误：无法解析产品");

        if (product.Status != "Active")
            return Fail($"产品 [{product.ProductCode}] 状态为 {product.Status}，无法过站");

        var station = await _stationRepo.GetByCode(request.StationCode);
        if (station == null)
            return Fail($"站点 [{request.StationCode}] 不存在");

        if (!isBarcodeMode)
        {
            var binding = await _bindingRepo.GetByProductId(product.Id);
            if (binding == null)
                return Fail($"产品 [{request.ProductCode}] 未绑定流程，请在流程配置中绑定");

            flow = await _flowRepo.GetById(binding.FlowId);
            if (flow == null || !flow.IsActive)
                return Fail($"绑定的流程 [{binding.FlowId}] 无效或已停用");
        }

        if (isBarcodeMode && flow == null)
        {
            var binding = await _bindingRepo.GetByProductId(product.Id);
            if (binding == null)
                return Fail($"产品 [{product.ProductCode}] 未绑定流程，请在流程配置中绑定");

            flow = await _flowRepo.GetById(binding.FlowId);
            if (flow == null || !flow.IsActive)
                return Fail($"绑定的流程 [{binding.FlowId}] 无效或已停用");
        }

        if (flow == null)
            return Fail($"产品 [{product.ProductCode}] 未能确定有效流程，请检查流程绑定或来料FlowId");

        var flowStations = JsonSerializer.Deserialize<List<FlowStationItem>>(flow.FlowJson)
            ?? new List<FlowStationItem>();

        if (flowStations.Count == 0)
            return Fail($"流程 [{flow.FlowCode}] 未定义任何站点");

        var currentStationInFlow = flowStations.FirstOrDefault(s => s.StationId == station.Id);
        if (currentStationInFlow == null)
            return Fail($"站点 [{station.StationCode}] 不在产品 [{product.ProductCode}] 的流程 [{flow.FlowCode}] 中");

        if (isBarcodeMode)
        {
            var expectedStationId = barcode!.CurrentStationId
                ?? flowStations.OrderBy(s => s.Order).First().StationId;

            if (expectedStationId != station.Id)
            {
                var expectedStation = await _stationRepo.GetById((int)expectedStationId);
                return Fail($"条码应在站点 [{expectedStation?.StationCode ?? "?"}] 过站，当前扫描站点为 [{station.StationCode}]");
            }
        }

        var rules = await _ruleRepo.GetActiveByStation(station.Id);
        var matchedRules = new List<GateRuleInfo>();

        foreach (var rule in rules)
        {
            if (EvaluateRule(rule, product))
            {
                matchedRules.Add(new GateRuleInfo
                {
                    RuleId = rule.Id,
                    RuleName = rule.RuleName,
                    RuleType = rule.RuleType,
                    Condition = rule.Condition
                });
            }
        }

        string passResult;
        string message;

        if (matchedRules.Any(r => r.RuleType == "Block"))
        {
            passResult = "Block";
            message = isBarcodeMode
                ? $"条码 [{barcode!.Barcode}]（产品 [{product.ProductCode}]）在站点 [{station.StationCode}] 被卡关"
                : $"产品 [{product.ProductCode}] 在站点 [{station.StationCode}] 被卡关";
        }
        else if (matchedRules.Any(r => r.RuleType == "Warn"))
        {
            passResult = "Pass";
            message = isBarcodeMode
                ? $"条码 [{barcode!.Barcode}] 通过站点 [{station.StationCode}]，有警告"
                : $"产品 [{product.ProductCode}] 通过站点 [{station.StationCode}]，有警告";
        }
        else if (matchedRules.Any(r => r.RuleType == "Skip"))
        {
            passResult = "Skip";
            message = isBarcodeMode
                ? $"条码 [{barcode!.Barcode}] 跳过站点 [{station.StationCode}]"
                : $"产品 [{product.ProductCode}] 跳过站点 [{station.StationCode}]";
        }
        else
        {
            passResult = "Pass";
            message = isBarcodeMode
                ? $"条码 [{barcode!.Barcode}] 通过站点 [{station.StationCode}]"
                : $"产品 [{product.ProductCode}] 通过站点 [{station.StationCode}]";
        }

        var currentOrder = currentStationInFlow.Order;
        var nextStationItem = flowStations
            .Where(s => s.Order > currentOrder)
            .OrderBy(s => s.Order)
            .FirstOrDefault();

        Station? nextStationEntity = null;
        if (nextStationItem != null)
        {
            nextStationEntity = await _stationRepo.GetById(nextStationItem.StationId);
        }

        _logger.LogInformation("GateCheck: Product={Product}, Station={Station}, Result={Result}, Barcode={Barcode}",
            request.ProductCode, request.StationCode, passResult, barcode?.Barcode ?? "-");

        return new GateCheckResult
        {
            Passed = passResult == "Pass" || passResult == "Skip" || passResult == "Warn",
            Result = passResult,
            Message = message,
            ProductCode = product.ProductCode,
            StationCode = station.StationCode,
            StationName = station.StationName,
            NextStationId = nextStationItem?.StationId,
            NextStationCode = nextStationEntity?.StationCode,
            NextStationName = nextStationEntity?.StationName,
            MatchedRules = matchedRules,
            BarcodeId = barcode?.Id,
            Barcode = barcode?.Barcode
        };
    }

    private static bool EvaluateRule(GateRule rule, Product product)
    {
        try
        {
            using var doc = JsonDocument.Parse(rule.Condition);
            var root = doc.RootElement;

            if (!root.TryGetProperty("Field", out var fieldEl) ||
                !root.TryGetProperty("Operator", out var opEl) ||
                !root.TryGetProperty("Value", out var valEl))
                return false;

            var field = fieldEl.GetString() ?? "";
            var op = opEl.GetString() ?? "";
            var value = valEl.GetString() ?? "";

            var fieldValue = field switch
            {
                "BatchNo" => product.BatchNo ?? "",
                "Status" => product.Status,
                "ProductCode" => product.ProductCode,
                _ => ""
            };

            return op switch
            {
                "Equals" => string.Equals(fieldValue, value, StringComparison.OrdinalIgnoreCase),
                "Contains" => fieldValue.Contains(value, StringComparison.OrdinalIgnoreCase),
                "NotEquals" => !string.Equals(fieldValue, value, StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }
        catch
        {
            return false;
        }
    }

    private static GateCheckResult Fail(string message)
        => new GateCheckResult { Passed = false, Result = "Error", Message = message };
}
