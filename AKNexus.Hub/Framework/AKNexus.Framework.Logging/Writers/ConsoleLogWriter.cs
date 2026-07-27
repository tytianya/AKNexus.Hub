using AKNexus.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKNexus.Framework.Logging.Writers
{
    //替代默认 ConsoleLogger 的实现
    public class ConsoleLogWriter : ILogWriter
    {
        private readonly CustomLoggingOptions _options;

        public ConsoleLogWriter(IOptions<CustomLoggingOptions> options)
        {
            _options = options.Value;
        }

        public Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default)
        {
            if (!_options.EnableConsole) return Task.CompletedTask;

            // 根据不同级别设置控制台颜色，美化输出
            var color = entry.LogLevel switch
            {
                LogLevel.Critical => ConsoleColor.DarkRed,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Information => ConsoleColor.Green,
                _ => ConsoleColor.Gray
            };

            Console.ForegroundColor = color;
            Console.WriteLine($"[{entry.Timestamp:HH:mm:ss}] [{entry.LogLevel}] [{entry.CategoryName}] {entry.Message}");
            if (entry.Exception != null)
            {
                Console.WriteLine(entry.Exception);
            }
            Console.ResetColor();

            return Task.CompletedTask;
        }
    }
}
