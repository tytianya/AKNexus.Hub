namespace AKNexus.Core.Logging;

using Microsoft.Extensions.Logging;

// 与 appsettings.json 中的 CustomLogging 节点对应
public class CustomLoggingOptions
{
    public bool EnableConsole { get; set; } = true;
    public bool EnableFile { get; set; } = true;
    public string FilePath { get; set; } = "logs/hub.log";
    public int MaxFileSizeMB { get; set; } = 20;// 单个日志文件超过 20MB 自动分卷
    public int RetainedDays { get; set; } = 30;// 自动清理 15 天前的旧日志
}

// 统一的日志实体
public struct LogEntry
{
    public LogLevel LogLevel { get; set; }
    public string CategoryName { get; set; }
    public EventId EventId { get; set; }
    public string Message { get; set; }
    public Exception? Exception { get; set; }
    public DateTime Timestamp { get; init; }
    /// <summary>
    /// 原始结构化参数
    /// </summary>
    public object? State { get; init; }

    /// <summary>
    /// Scope信息
    /// </summary>
    public IReadOnlyList<object>? Scopes { get; init; }
}

