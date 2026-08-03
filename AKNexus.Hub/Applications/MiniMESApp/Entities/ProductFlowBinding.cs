using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMESApp.Entities;

[Table("ProductFlowBinding")]
public class ProductFlowBinding
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public int ProductId { get; set; }
    [Required]
    public int FlowId { get; set; }
    [Required]
    public DateTime BoundAt { get; set; } = DateTime.UtcNow;
}
