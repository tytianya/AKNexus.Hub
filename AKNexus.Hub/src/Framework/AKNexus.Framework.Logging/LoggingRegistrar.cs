using AKNexus.Core;
using AKNexus.Framework.Logging.Extensions;
using AKNexus.Framework.Logging.Writers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: AssemblyRegistrar(typeof(LoggingRegistrar))]
namespace AKNexus.Framework.Logging;

/// <summary>
/// 日志组件注册器
/// </summary>
/// <remarks>
/// 设计原则：
/// 1. 替换控制台和 Debug 输出为自定义方式
/// 2. 保留 EventLog 和 EventSource（系统事件）
/// 3. 兼容 appsettings.json 的 Logging.LogLevel 配置
/// </remarks>
public sealed class LoggingRegistrar : IAssemblyRegistrar
{
    public int Order => 0;

    public void Configure(IKernelBuilder builder)
    {
        var configuration = builder.Configuration;
        var services = builder.Services;

        // 1. 读取自定义配置
        var options = configuration.GetSection("CustomLogging").Get<CustomLoggingOptions>()
            ?? new CustomLoggingOptions();

        // 2. 注册日志写入器
        if (options.EnableConsole)
        {
            services.AddSingleton<Abstractions.ILogWriter, ConsoleLogWriter>();
        }

        if (options.EnableFile)
        {
            services.AddSingleton<Abstractions.ILogWriter>(sp =>
                new FileLogWriter(
                    options.FilePath,
                    options.MaxFileSizeMB,
                    options.RetainedFileCount));
        }

        // 3. 替换日志 Provider
        services.AddLogging(logging =>
        {
            // 清除原有 Provider（Console、Debug）
            logging.ClearProviders();

            // 重新添加需要保留的系统 Provider
            if (OperatingSystem.IsWindows())
            {
                // Windows 事件日志（保留）
                logging.AddEventLog();
            }

            // TraceSource（保留，用于 ETW）
            // logging.AddTraceSource();  // 如果需要

            // 添加自定义 Provider（控制台、文件）
            logging.AddCustomLogging();
        });
    }
}