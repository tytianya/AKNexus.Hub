using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMESApp.Entities;

[Table("StationSoftwareBinding")]
public class StationSoftwareBinding
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public int StationId { get; set; }
    [Required]
    public int SoftwareId { get; set; }
    [Required]
    public DateTime BoundAt { get; set; } = DateTime.UtcNow;
}
