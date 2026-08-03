using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMESApp.Entities;

[Table("IncomingMaterial")]
public class IncomingMaterial
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string MaterialCode { get; set; } = "";
    [Required]
    public string ProductModel { get; set; } = "";
    [Required]
    public int Quantity { get; set; }
    public string? Supplier { get; set; }
    public string? LotNo { get; set; }
    public int? FlowId { get; set; }
    [Required]
    public string InspectResult { get; set; } = "Pending";
    public string? InspectedBy { get; set; }
    public DateTime? InspectedAt { get; set; }
    public string? Remark { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
