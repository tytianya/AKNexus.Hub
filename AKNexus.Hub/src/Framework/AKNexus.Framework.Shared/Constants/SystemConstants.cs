namespace AKNexus.Framework.Shared.Constants;

/// <summary>
/// 系统常量定义
/// </summary>
public static class SystemConstants
{
    /// <summary>
    /// 系统名称
    /// </summary>
    public const string SystemName = "AKNexus Hub";

    /// <summary>
    /// 系统版本
    /// </summary>
    public const string Version = "1.0.0";

    /// <summary>
    /// 默认分页大小
    /// </summary>
    public const int DefaultPageSize = 20;

    /// <summary>
    /// 最大分页大小
    /// </summary>
    public const int MaxPageSize = 100;
}

/// <summary>
/// 错误代码常量
/// </summary>
public static class ErrorCodes
{
    public const string NotFound = "NOT_FOUND";
    public const string BadRequest = "BAD_REQUEST";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string InternalError = "INTERNAL_ERROR";
    public const string ValidationFailed = "VALIDATION_FAILED";
}