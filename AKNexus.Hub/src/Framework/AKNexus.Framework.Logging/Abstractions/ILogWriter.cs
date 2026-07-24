namespace AKNexus.Framework.Logging.Abstractions;

/// <summary>
/// 日志写入接口
/// </summary>
public interface ILogWriter
{
    /// <summary>
    /// 写入日志
    /// </summary>
    Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default);
}