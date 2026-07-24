using AKNexus.Framework.Logging.Dtos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKNexus.Core.Log
{
    /// <summary>
    /// 静态日志：日志组件未注册前
    /// </summary>
    public static class StartupLogger
    {
        public static void Write(IConfiguration configuration, string message)
        {
            // 内部自动绑定配置
            var options = new CustomLoggingOptions();
            configuration.GetSection("Logging:AKNexusLog").Bind(options);
            var formattedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] [Information] [AKNexus.Framework.Logging] {message}";

            if (options.EnableConsole)
            {
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(formattedMessage);
                Console.ForegroundColor = originalColor;
            }

            if (options.EnableFile)
            {
                try
                {
                    var logDir = Path.GetDirectoryName(options.FilePath) ?? "logs";
                    if (!Directory.Exists(logDir))
                    {
                        Directory.CreateDirectory(logDir);
                    }

                    var todayStr = DateTime.Now.ToString("yyyyMMdd");
                    var targetPath = Path.Combine(logDir, $"aknexus_{todayStr}.log");

                    File.AppendAllText(targetPath, formattedMessage + Environment.NewLine, Encoding.UTF8);
                }
                catch
                {
                    // 忽略启动期文件占用
                }
            }
        }
    }
}
