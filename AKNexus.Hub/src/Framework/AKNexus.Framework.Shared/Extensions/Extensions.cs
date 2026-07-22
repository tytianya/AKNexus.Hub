namespace AKNexus.Framework.Shared.Extensions;

/// <summary>
/// 集合扩展方法
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// 判断集合是否为空或null
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }

    /// <summary>
    /// 判断集合是否不为空
    /// </summary>
    public static bool IsNotEmpty<T>(this IEnumerable<T>? source)
    {
        return source != null && source.Any();
    }

    /// <summary>
    /// 将集合转换为只读集合
    /// </summary>
    public static IReadOnlyList<T> AsReadOnlyList<T>(this IEnumerable<T> source)
    {
        return source.ToList().AsReadOnly();
    }

    /// <summary>
    /// 批量ForEach操作
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }
}

/// <summary>
/// 字符串扩展方法
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// 判断字符串是否为空或空白
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// 判断字符串是否不为空或空白
    /// </summary>
    public static bool IsNotNullOrWhiteSpace(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// 转换为Guid（失败返回Guid.Empty）
    /// </summary>
    public static Guid ToGuid(this string? value)
    {
        return Guid.TryParse(value, out var result) ? result : Guid.Empty;
    }

    /// <summary>
    /// 转换为Guid（失败返回null）
    /// </summary>
    public static Guid? ToGuidOrNull(this string? value)
    {
        return Guid.TryParse(value, out var result) ? result : null;
    }
}

/// <summary>
/// 日期时间扩展方法
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// 转换为时间戳（秒）
    /// </summary>
    public static long ToTimestamp(this DateTimeOffset dateTime)
    {
        return new DateTimeOffset(dateTime.DateTime).ToUnixTimeSeconds();
    }

    /// <summary>
    /// 转换为时间戳（毫秒）
    /// </summary>
    public static long ToTimestampMilliseconds(this DateTimeOffset dateTime)
    {
        return new DateTimeOffset(dateTime.DateTime).ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// 从时间戳转换为DateTimeOffset
    /// </summary>
    public static DateTimeOffset FromTimestamp(long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp);
    }

    /// <summary>
    /// 判断是否为今天
    /// </summary>
    public static bool IsToday(this DateTimeOffset dateTime)
    {
        return dateTime.Date == DateTimeOffset.UtcNow.Date;
    }
}