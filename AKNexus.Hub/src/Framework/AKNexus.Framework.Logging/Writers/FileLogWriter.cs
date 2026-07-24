using System.Collections.Concurrent;
using AKNexus.Framework.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace AKNexus.Framework.Logging.Writers;

/// <summary>
/// 文件日志写入器
/// </summary>
public sealed class FileLogWriter : ILogWriter, IDisposable
{
    private readonly string _filePath;
    private readonly string _basePath;
    private readonly int _maxFileSizeBytes;
    private readonly int _retainedFileCount;
    private readonly ConcurrentQueue<string> _queue = new();
    private readonly Timer _timer;
    private readonly object _writeLock = new();
    private int _currentFileSize;
    private int _currentIndex = 1;

    public FileLogWriter(
        string filePath,
        int maxFileSizeMB = 10,
        int retainedFileCount = 30)
    {
        _filePath = filePath;
        _basePath = Path.GetDirectoryName(filePath) ?? "logs";
        _maxFileSizeBytes = maxFileSizeMB * 1024 * 1024;
        _retainedFileCount = retainedFileCount;

        // 确保目录存在
        Directory.CreateDirectory(_basePath);

        // 定时刷新到文件
        _timer = new Timer(Flush, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    public Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default)
    {
        _queue.Enqueue(FormatEntry(entry));
        return Task.CompletedTask;
    }

    private void Flush(object? state)
    {
        if (_queue.IsEmpty)
            return;

        lock (_writeLock)
        {
            try
            {
                var currentFile = GetCurrentFile();
                using var writer = new StreamWriter(currentFile, append: true);

                while (_queue.TryDequeue(out var line))
                {
                    writer.WriteLine(line);
                    _currentFileSize += line.Length;
                }
            }
            catch
            {
                // 忽略写入失败
            }
        }
    }

    private string GetCurrentFile()
    {
        var file = _filePath;

        // 检查文件大小，超过限制则滚动
        if (File.Exists(file))
        {
            var fileInfo = new FileInfo(file);
            _currentFileSize = _currentFileSize == 0 ? (int)fileInfo.Length : _currentFileSize;

            if (_currentFileSize >= _maxFileSizeBytes)
            {
                _currentIndex++;
                _currentFileSize = 0;

                // 滚动文件名
                var fileName = Path.GetFileNameWithoutExtension(file);
                var ext = Path.GetExtension(file);
                file = Path.Combine(_basePath, $"{fileName}_{_currentIndex}{ext}");

                // 清理旧文件
                CleanupOldFiles();
            }
        }

        return file;
    }

    private void CleanupOldFiles()
    {
        try
        {
            var files = Directory.GetFiles(_basePath, "*.log")
                .OrderByDescending(f => f)
                .Skip(_retainedFileCount);

            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
        catch
        {
            // 忽略清理失败
        }
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

        var line = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {entry.Category}: {entry.Message}";

        if (entry.Exception != null)
        {
            line += Environment.NewLine + entry.Exception.ToString();
        }

        return line;
    }

    public void Dispose()
    {
        _timer.Dispose();
        Flush(null);
    }
}