using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMESApp.Entities;

[Table("Product")]
public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string ProductCode { get; set; } = string.Empty;
    [Required]
    public string ProductName { get; set; } = string.Empty;
    public string? Spec { get; set; }
    public string? BatchNo { get; set; }
    [Required]
    public string Status { get; set; } = "Active";
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
