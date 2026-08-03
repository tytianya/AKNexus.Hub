using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMESApp.Entities;

[Table("MacRange")]
public class MacRange
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string RangeName { get; set; } = string.Empty;
    [Required]
    public string StartMac { get; set; } = string.Empty;
    [Required]
    public string EndMac { get; set; } = string.Empty;
    [Required]
    public string NextMac { get; set; } = string.Empty;
    [Required]
    public bool IsActive { get; set; } = true;
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
