namespace AKNexus.Framework.Logging.Abstractions;

/// <summary>
/// 日志条目
/// </summary>
public class LogEntry
{
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// 日志类别
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// 日志级别
    /// </summary>
    public LogLevel Level { get; init; }

    /// <summary>
    /// 日志消息
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// 异常信息
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// 事件ID
    /// </summary>
    public int EventId { get; init; }

    /// <summary>
    /// 事件名称
    /// </summary>
    public string? EventName { get; init; }

    /// <summary>
    /// 追踪ID
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// SpanID
    /// </summary>
    public string? SpanId { get; init; }

    /// <summary>
    /// 扩展属性
    /// </summary>
    public IDictionary<string, object?> Properties { get; init; } = new Dictionary<string, object?>();
}