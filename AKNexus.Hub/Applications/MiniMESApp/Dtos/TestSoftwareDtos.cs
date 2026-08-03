namespace MiniMESApp.Dtos;

public class TestSoftwareDto
{
    public int Id { get; set; }
    public string SoftwareName { get; set; } = string.Empty;
    public string SoftwareCode { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class SoftwareBindRequest
{
    public int StationId { get; set; }
    public int SoftwareId { get; set; }
}

public class StationSoftwareDto
{
    public int Id { get; set; }
    public string SoftwareName { get; set; } = string.Empty;
    public string SoftwareCode { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public DateTime BoundAt { get; set; }
    public string? StationName { get; set; }
    public string? StationCode { get; set; }
}
