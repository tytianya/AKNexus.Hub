namespace MiniMESApp.Dtos;

public class ProductStationLogDto
{
    public long Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string StationCode { get; set; } = string.Empty;
    public string StationName { get; set; } = string.Empty;
    public string FlowCode { get; set; } = string.Empty;
    public string PassResult { get; set; } = string.Empty;
    public string? Remark { get; set; }
    public string? Operator { get; set; }
    public DateTime PassTime { get; set; }
}

public class ProductQueryRequest
{
    public string? ProductCode { get; set; }
    public string? StationCode { get; set; }
    public string? FlowCode { get; set; }
    public string? PassResult { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
