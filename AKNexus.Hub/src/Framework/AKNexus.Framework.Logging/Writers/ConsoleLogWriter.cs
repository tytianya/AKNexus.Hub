using System.Collections.Concurrent;
using AKNexus.Framework.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace AKNexus.Framework.Logging.Writers;

/// <summary>
/// 控制台日志写入器
/// </summary>
public sealed class ConsoleLogWriter : ILogWriter
{
    private static readonly object _lock = new();

    public Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default)
    {
        var color = GetConsoleColor(entry.Level);

        lock (_lock)
        {
            var originalColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine(FormatEntry(entry));

                if (entry.Exception != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(entry.Exception.ToString());
                }
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }

        return Task.CompletedTask;
    }

    private static string FormatEntry(LogEntry entry)
    {
        var level = entry.Level switch
        {
            LogLevel.Trace => "TRCE",
            LogLevel.Debug => "DBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "FAIL",
            LogLevel.Critical => "CRIT",
            _ => "????"
        };

        return $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {entry.Category}: {entry.Message}";
    }

    private static ConsoleColor GetConsoleColor(LogLevel level) => level switch
    {
        LogLevel.Trace => ConsoleColor.DarkGray,
        LogLevel.Debug => ConsoleColor.Gray,
        LogLevel.Information => ConsoleColor.White,
        LogLevel.Warning => ConsoleColor.Yellow,
        LogLevel.Error => ConsoleColor.Red,
        LogLevel.Critical => ConsoleColor.DarkRed,
        _ => ConsoleColor.White
    };
}