using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMESApp.Entities;

[Table("ProductStationLog")]
public class ProductStationLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [Required]
    public int ProductId { get; set; }
    [Required]
    public int StationId { get; set; }
    [Required]
    public int FlowId { get; set; }
    [Required]
    public string PassResult { get; set; } = "Pass";
    public long? BarcodeId { get; set; }
    public string? Remark { get; set; }
    public string? Operator { get; set; }
    [Required]
    public DateTime PassTime { get; set; } = DateTime.UtcNow;
}
