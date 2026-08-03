using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMESApp.Entities;

[Table("TestSoftware")]
public class TestSoftware
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string SoftwareName { get; set; } = string.Empty;
    [Required]
    public string SoftwareCode { get; set; } = string.Empty;
    [Required]
    public string Version { get; set; } = string.Empty;
    [Required]
    public string FileName { get; set; } = string.Empty;
    [Required]
    public string FilePath { get; set; } = string.Empty;
    [Required]
    public long FileSize { get; set; }
    public string? Description { get; set; }
    [Required]
    public bool IsActive { get; set; } = true;
    [Required]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
