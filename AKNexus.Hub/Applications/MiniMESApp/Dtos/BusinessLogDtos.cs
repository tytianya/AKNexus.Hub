using AKNexus.Framework.Abstractions.Dtos;
using MiniMESApp.Entities;
using System.Text.Json;

namespace MiniMESApp.Dtos;

/// <summary>
/// 业务日志列表 DTO
/// </summary>
public class BusinessLogListDto
{
    public int Id { get; set; }
    public BusinessType Type { get; set; }
    public string TypeName { get; set; } = "";
    public string Code { get; set; } = "";
    public string Content { get; set; } = "";
    public string? Remark { get; set; }
    public BusinessStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 业务日志分页查询
/// </summary>
public class BusinessLogQueryDto : PagedRequest
{
    /// <summary>
    /// 业务类型筛选（可选）
    /// </summary>
    public BusinessType? Type { get; set; }

    /// <summary>
    /// 状态筛选（可选）
    /// </summary>
    public BusinessStatus? Status { get; set; }

    /// <summary>
    /// 关键词搜索（业务编码、业务内容）
    /// </summary>
    public string? Keyword { get; set; }
}

/// <summary>
/// 业务日志推送输入
/// </summary>
public class BusinessLogPushInput
{
    /// <summary>
    /// 业务类型
    /// </summary>
    public BusinessType Type { get; set; }

    /// <summary>
    /// 业务内容（动态类型：字符串直接存储，对象/数组存 JSON）
    /// </summary>
    public object Content { get; set; } = "";

    /// <summary>
    /// 描述
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 成功或失败
    /// </summary>
    public BusinessStatus Status { get; set; }

    /// <summary>
    /// 将动态 Content 转为存储字符串
    /// </summary>
    public string ResolveContent()
    {
        return Content switch
        {
            null => "",
            string s => s,
            JsonElement je => je.ValueKind == JsonValueKind.String
                ? je.GetString() ?? ""
                : je.GetRawText(),
            _ => JsonSerializer.Serialize(Content)
        };
    }
}
