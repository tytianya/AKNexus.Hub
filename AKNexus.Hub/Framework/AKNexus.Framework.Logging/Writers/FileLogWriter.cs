using AKNexus.Core.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKNexus.Framework.Logging.Writers
{
    //替代默认 FileLogger 的实现
    /// <summary>
    /// 工业级文件写入器（长连接 Stream + 动态句柄状态机 + 内部攒批 Flush + 自动清理）
    /// 严格适配 ILogWriter 接口：WriteAsync(LogEntry entry, ...)
    /// </summary>
    public class FileLogWriter : ILogWriter, IDisposable
    {
        private readonly CustomLoggingOptions _options;

        // 💡 长连接句柄，避免频繁 Open/Close
        private StreamWriter? _currentWriter;
        private string _currentDateStr = string.Empty;
        private int _currentFileIndex = 0;
        private long _currentFileBytes = 0;
        private DateTime _lastCleanupTime = DateTime.MinValue;

        // 💡 内部攒批计数器，减少磁盘 Flush 频率
        private int _unflushedCount = 0;
        private const int AutoFlushThreshold = 50; // 每写 50 条自动 Flush 一次

        public FileLogWriter(IOptions<CustomLoggingOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// 接口实现：单条写入（内部自动处理句柄切换与攒批 Flush）
        /// </summary>
        public async Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default)
        {
            if (!_options.EnableFile) return;

            // 1. 确保句柄有效（换天、超大分卷）
            EnsureWriterInitialized(entry.Timestamp);
            if (_currentWriter == null) return;

            // 2. 格式化并写入长连接流
            var line = $"[{entry.Timestamp:HH:mm:ss.fff}] [{entry.LogLevel}] [{entry.CategoryName}] {entry.Message}";
            _currentWriter.WriteLine(line);
            _currentFileBytes += Encoding.UTF8.GetByteCount(line) + 2;

            if (entry.Exception != null)
            {
                var exStr = entry.Exception.ToString();
                _currentWriter.WriteLine(exStr);
                _currentFileBytes += Encoding.UTF8.GetByteCount(exStr) + 2;
            }

            // 3. 内部攒批控制：达到阀门或收到 Error 级别时强制 Flush 刷盘
            _unflushedCount++;
            if (_unflushedCount >= AutoFlushThreshold || entry.LogLevel >= Microsoft.Extensions.Logging.LogLevel.Error)
            {
                await _currentWriter.FlushAsync(cancellationToken);
                _unflushedCount = 0;
            }

            // 4. 检测并触发旧日志清理
            TryCleanupOldFiles();
        }

        /// <summary>
        /// 句柄状态机：仅在【跨天】或【超过 MaxFileSizeMB】时切流
        /// </summary>
        private void EnsureWriterInitialized(DateTime targetTime)
        {
            var todayStr = targetTime.ToString("yyyyMMdd");
            var maxBytes = _options.MaxFileSizeMB * 1024 * 1024;

            bool dateChanged = _currentDateStr != todayStr;
            bool sizeExceeded = _currentFileBytes >= maxBytes;

            // 复用常驻句柄
            if (_currentWriter != null && !dateChanged && !sizeExceeded)
            {
                return;
            }

            // 先把旧句柄里的东西刷完并释放
            CloseCurrentWriter();

            if (dateChanged)
            {
                _currentDateStr = todayStr;
                _currentFileIndex = 0;
            }
            else if (sizeExceeded)
            {
                _currentFileIndex++;
            }

            var logDir = Path.GetDirectoryName(_options.FilePath) ?? "logs";
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

            var fileName = _currentFileIndex == 0
                ? $"aknexus_{_currentDateStr}.log"
                : $"aknexus_{_currentDateStr}_{_currentFileIndex}.log";

            var fullPath = Path.Combine(logDir, fileName);

            var fileInfo = new FileInfo(fullPath);
            _currentFileBytes = fileInfo.Exists ? fileInfo.Length : 0;

            // 打开长连接 FileStream
            var fileStream = new FileStream(
                fullPath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);

            _currentWriter = new StreamWriter(fileStream, Encoding.UTF8);
            _unflushedCount = 0;
        }

        /// <summary>
        /// 自动清理超出 RetainedDays 天数的旧文件
        /// </summary>
        private void TryCleanupOldFiles()
        {
            if ((DateTime.Now - _lastCleanupTime).TotalHours < 24) return;
            _lastCleanupTime = DateTime.Now;

            Task.Run(() =>
            {
                try
                {
                    var logDir = Path.GetDirectoryName(_options.FilePath) ?? "logs";
                    if (!Directory.Exists(logDir)) return;

                    var thresholdDate = DateTime.Now.AddDays(-_options.RetainedDays);
                    var dirInfo = new DirectoryInfo(logDir);

                    foreach (var file in dirInfo.GetFiles("aknexus_*.log"))
                    {
                        if (file.LastWriteTime < thresholdDate)
                        {
                            file.Delete();
                        }
                    }
                }
                catch
                {
                    // 静默忽略占用无法删除的情况
                }
            });
        }

        private void CloseCurrentWriter()
        {
            if (_currentWriter != null)
            {
                _currentWriter.Flush();
                _currentWriter.Dispose();
                _currentWriter = null;
                _unflushedCount = 0;
            }
        }

        public void Dispose()
        {
            CloseCurrentWriter();
        }
    }
}
