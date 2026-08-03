using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMESApp.Entities;

[Table("StationFlow")]
public class StationFlow
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string FlowCode { get; set; } = string.Empty;
    [Required]
    public string FlowName { get; set; } = string.Empty;
    [Required]
    [Column(TypeName = "text")]
    public string FlowJson { get; set; } = "[]";
    public string? Description { get; set; }
    [Required]
    public bool IsActive { get; set; } = true;
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
