using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMESApp.Entities;

[Table("MacAllocation")]
public class MacAllocation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [Required]
    public int MacRangeId { get; set; }
    [Required]
    public string MacAddress { get; set; } = string.Empty;
    public string? AllocatedTo { get; set; }
    [Required]
    public DateTime AllocatedAt { get; set; } = DateTime.UtcNow;
}
