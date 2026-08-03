namespace MiniMESApp.Dtos;

public class IncomingMaterialCreateDto
{
    public int Id { get; set; }
    public string MaterialCode { get; set; } = "";
    public string ProductModel { get; set; } = "";
    public int Quantity { get; set; }
    public int? FlowId { get; set; }
    public string? Supplier { get; set; }
    public string? LotNo { get; set; }
    public string? Remark { get; set; }
}

public class IncomingMaterialUpdateDto
{
    public int Id { get; set; }
    public string InspectResult { get; set; } = "Pending";
    public string? InspectedBy { get; set; }
    public string? Remark { get; set; }
}

public class IncomingMaterialListDto
{
    public int Id { get; set; }
    public string MaterialCode { get; set; } = "";
    public string ProductModel { get; set; } = "";
    public int Quantity { get; set; }
    public int BarcodeCount { get; set; }
    public string? Supplier { get; set; }
    public string? LotNo { get; set; }
    public string InspectResult { get; set; } = "Pending";
    public string? InspectedBy { get; set; }
    public DateTime? InspectedAt { get; set; }
    public string? Remark { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MaterialBarcodeDto
{
    public long Id { get; set; }
    public int IncomingMaterialId { get; set; }
    public int SequenceNo { get; set; }
    public string Barcode { get; set; } = "";
    public long? CurrentStationId { get; set; }
    public string? StationName { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
