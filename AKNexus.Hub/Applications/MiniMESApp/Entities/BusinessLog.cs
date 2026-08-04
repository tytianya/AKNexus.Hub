using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMESApp.Entities;

/// <summary>
/// 业务类型枚举
/// </summary>
public enum BusinessType
{
    /// <summary>
    /// 条码测试
    /// </summary>
    BarcodeTest = 1,

    /// <summary>
    /// 站点测试
    /// </summary>
    StationTest = 2
}

/// <summary>
/// 业务状态枚举
/// </summary>
public enum BusinessStatus
{
    /// <summary>
    /// 成功
    /// </summary>
    Success = 1,

    /// <summary>
    /// 失败
    /// </summary>
    Failed = 2
}

/// <summary>
/// 业务日志
/// </summary>
[Table("BusinessLog")]
public class BusinessLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    public BusinessType Type { get; set; }

    /// <summary>
    /// 业务编码（后台根据 Type 自动映射）
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = "";

    /// <summary>
    /// 业务内容（动态类型，存 JSON 字符串）
    /// </summary>
    [Required]
    public string Content { get; set; } = "";

    /// <summary>
    /// 描述
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 成功或失败
    /// </summary>
    public BusinessStatus Status { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
