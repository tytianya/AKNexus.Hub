using AKNexus.Framework.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace AKNexus.Framework.Infrastructure.Entities;

/// <summary>
/// 用户实体
/// </summary>
public class User : FullAuditEntity
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 密码哈希
    /// </summary>
    [Required]
    [StringLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 名
    /// </summary>
    [StringLength(50)]
    public string? FirstName { get; set; }

    /// <summary>
    /// 姓
    /// </summary>
    [StringLength(50)]
    public string? LastName { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    [StringLength(20)]
    public string? Phone { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public int Status { get; set; } = 1;
}

/// <summary>
/// 角色实体
/// </summary>
public class Role : EntityBase
{
    /// <summary>
    /// 角色名
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    [StringLength(200)]
    public string? Description { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public int Status { get; set; } = 1;
}

/// <summary>
/// 用户角色关联实体
/// </summary>
public class UserRole : EntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// 用户导航属性
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// 角色导航属性
    /// </summary>
    public Role Role { get; set; } = null!;
}