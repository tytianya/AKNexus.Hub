namespace AKNexus.Framework.Logging;

using AKNexus.Core.Logging;
using Microsoft.Extensions.Logging;

public class AKNexusLogger : ILogger
{
    private readonly string _categoryName;
    private readonly LogMessageQueue _queue;

    public AKNexusLogger(string categoryName, LogMessageQueue queue)
    {
        _categoryName = categoryName;
        _queue = queue;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    // 只要系统传过来了，就说明它已经通过了 appsettings.json 中 LogLevel 的校验
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        _queue.Channel.Writer.TryWrite(new LogEntry
        {
            LogLevel = logLevel,
            CategoryName = _categoryName,
            EventId = eventId,
            Message = formatter(state, exception),
            Exception = exception,
            Timestamp = DateTime.Now
        });
    }
}