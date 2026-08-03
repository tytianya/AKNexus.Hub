using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMESApp.Entities;

[Table("MaterialBarcode")]
public class MaterialBarcode
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [Required]
    public int IncomingMaterialId { get; set; }
    [Required]
    public int SequenceNo { get; set; }
    [Required]
    public string Barcode { get; set; } = "";
    public long? CurrentStationId { get; set; }
    [Required]
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
