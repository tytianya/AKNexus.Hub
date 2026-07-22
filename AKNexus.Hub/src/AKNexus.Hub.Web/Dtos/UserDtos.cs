using System.ComponentModel.DataAnnotations;

namespace AKNexus.Hub.Web.Dtos;

/// <summary>
/// 用户DTO
/// </summary>
public class UserDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 全名
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedTime { get; set; }
}

/// <summary>
/// 用户创建输入
/// </summary>
public class UserCreateInput
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required(ErrorMessage = "用户名不能为空")]
    [StringLength(50, ErrorMessage = "用户名长度不能超过50个字符")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    [Required(ErrorMessage = "邮箱不能为空")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    [StringLength(100, ErrorMessage = "邮箱长度不能超过100个字符")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须在6-100个字符之间")]
    public string Password { get; set; } = string.Empty;

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
}

/// <summary>
/// 用户更新输入
/// </summary>
public class UserUpdateInput
{
    /// <summary>
    /// 邮箱
    /// </summary>
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    [StringLength(100, ErrorMessage = "邮箱长度不能超过100个字符")]
    public string? Email { get; set; }

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
    public int? Status { get; set; }
}