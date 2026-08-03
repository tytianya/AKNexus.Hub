namespace MiniMESApp.Dtos;

public class MacRangeDto
{
    public int Id { get; set; }
    public string RangeName { get; set; } = string.Empty;
    public string StartMac { get; set; } = string.Empty;
    public string EndMac { get; set; } = string.Empty;
    public string NextMac { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public int AllocatedCount { get; set; }
    public int RemainingCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MacRangeCreateDto
{
    public string RangeName { get; set; } = string.Empty;
    public string StartMac { get; set; } = string.Empty;
    public string EndMac { get; set; } = string.Empty;
}

public class MacAllocateRequest
{
    public string? AllocatedTo { get; set; }
}

public class MacAllocationDto
{
    public long Id { get; set; }
    public string MacAddress { get; set; } = string.Empty;
    public string RangeName { get; set; } = string.Empty;
    public string? AllocatedTo { get; set; }
    public DateTime AllocatedAt { get; set; }
}
