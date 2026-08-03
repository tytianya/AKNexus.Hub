namespace MiniMESApp.Dtos;

public class StationCreateDto
{
    public string StationCode { get; set; } = string.Empty;
    public string StationName { get; set; } = string.Empty;
    public string StationType { get; set; } = "Check";
    public string? Location { get; set; }
    public int SortOrder { get; set; }
}
