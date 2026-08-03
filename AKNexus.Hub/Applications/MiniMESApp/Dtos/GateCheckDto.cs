namespace MiniMESApp.Dtos;

public class GateCheckRequest
{
    public string ProductCode { get; set; } = string.Empty;
    public string StationCode { get; set; } = string.Empty;
    public string? Operator { get; set; }
    public string? Remark { get; set; }
}

public class GateCheckResult
{
    public bool Passed { get; set; }
    public string Result { get; set; } = "Pass";
    public string Message { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public string? StationCode { get; set; }
    public string? StationName { get; set; }
    public int? NextStationId { get; set; }
    public string? NextStationCode { get; set; }
    public string? NextStationName { get; set; }
    public List<GateRuleInfo> MatchedRules { get; set; } = new();
    public long? BarcodeId { get; set; }
    public string? Barcode { get; set; }
}

public class GateRuleInfo
{
    public int RuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
}
