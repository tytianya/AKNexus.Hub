namespace MiniMESApp.Dtos;

public class StationDto
{
    public int Id { get; set; }
    public string StationCode { get; set; } = string.Empty;
    public string StationName { get; set; } = string.Empty;
    public string StationType { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int SortOrder { get; set; }
}
