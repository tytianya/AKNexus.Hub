namespace MiniMESApp.Dtos;

public class ProductDto
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Spec { get; set; }
    public string? BatchNo { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int? BoundFlowId { get; set; }
    public string? BoundFlowCode { get; set; }
}

public class ProductUpdateDto
{
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public string? Spec { get; set; }
    public string? BatchNo { get; set; }
    public string? Status { get; set; }
}
