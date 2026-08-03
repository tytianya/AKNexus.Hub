using System.Text.Json.Serialization;

namespace MiniMESApp.Dtos;

public class StationFlowDto
{
    public int Id { get; set; }
    public string FlowCode { get; set; } = string.Empty;
    public string FlowName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<FlowStationItem> Stations { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class FlowStationItem
{
    [JsonPropertyName("stationId")]
    public int StationId { get; set; }

    [JsonPropertyName("stationCode")]
    public string StationCode { get; set; } = string.Empty;

    [JsonPropertyName("stationName")]
    public string StationName { get; set; } = string.Empty;

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("required")]
    public bool Required { get; set; } = true;
}
