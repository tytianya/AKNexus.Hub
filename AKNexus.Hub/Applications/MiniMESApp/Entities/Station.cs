using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMESApp.Entities;

[Table("Station")]
public class Station
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string StationCode { get; set; } = string.Empty;
    [Required]
    public string StationName { get; set; } = string.Empty;
    [Required]
    public string StationType { get; set; } = "Check";
    public string? Location { get; set; }
    [Required]
    public int SortOrder { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
