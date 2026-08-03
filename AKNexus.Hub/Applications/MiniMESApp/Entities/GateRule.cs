using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMESApp.Entities;

[Table("GateRule")]
public class GateRule
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public int StationId { get; set; }
    [Required]
    public string RuleName { get; set; } = string.Empty;
    [Required]
    public string RuleType { get; set; } = "Block";
    [Required]
    [Column(TypeName = "text")]
    public string Condition { get; set; } = "{}";
    [Required]
    public bool IsActive { get; set; } = true;
    [Required]
    public int Priority { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
