using AKNexus.Framework.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace AKNexus.Framework.Logging;

/// <summary>
/// 自定义日志器 - 实现 ILogger，兼容 ASP.NET Core 原生日志配置
/// </summary>
internal sealed class CustomLogger : ILogger
{
    private readonly string _categoryName;
    private readonly IEnumerable<ILogWriter> _writers;
    private readonly IExternalScopeProvider? _scopeProvider;

    public CustomLogger(
        string categoryName,
        IEnumerable<ILogWriter> writers,
        IExternalScopeProvider? scopeProvider = null)
    {
        _categoryName = categoryName;
        _writers = writers;
        _scopeProvider = scopeProvider;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return _scopeProvider?.Push(state) ?? null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        // 这里只做基础判断，具体过滤由 Provider 层处理
        return logLevel != LogLevel.None;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message) && exception == null)
            return;

        var entry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Category = _categoryName,
            Level = logLevel,
            Message = message,
            Exception = exception,
            EventId = eventId.Id,
            EventName = eventId.Name
        };

        // 提取 Scope 信息
        _scopeProvider?.ForEachScope((scope, state) =>
        {
            if (scope is IEnumerable<KeyValuePair<string, object>> properties)
            {
                foreach (var kvp in properties)
                {
                    entry.Properties[kvp.Key] = kvp.Value;
                }
            }
        }, state);

        // 异步写入（不阻塞调用方）
        _ = WriteToAllWritersAsync(entry);
    }

    private async Task WriteToAllWritersAsync(LogEntry entry)
    {
        foreach (var writer in _writers)
        {
            try
            {
                await writer.WriteAsync(entry);
            }
            catch
            {
                // 忽略单个 Writer 失败，避免影响其他 Writer
            }
        }
    }
}