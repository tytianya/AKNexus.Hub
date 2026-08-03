namespace MiniMESApp.Dtos;

public class GateRuleDto
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public string StationCode { get; set; } = string.Empty;
    public string StationName { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Priority { get; set; }
}
