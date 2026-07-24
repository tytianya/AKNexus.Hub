using AKNexus.Framework.Logging.Abstractions;
using AKNexus.Framework.Logging.Writers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AKNexus.Framework.Logging.Extensions;

/// <summary>
/// 日志构建器扩展
/// </summary>
public static class LoggingBuilderExtensions
{
    /// <summary>
    /// 添加自定义日志框架（兼容 ASP.NET Core 原生配置）
    /// </summary>
    public static ILoggingBuilder AddCustomLogging(
        this ILoggingBuilder builder,
        Action<CustomLoggingOptions>? configure = null)
    {
        var options = new CustomLoggingOptions();
        configure?.Invoke(options);

        // 注册 Writer
        if (options.EnableConsole)
        {
            builder.Services.AddSingleton<ILogWriter, ConsoleLogWriter>();
        }

        if (options.EnableFile)
        {
            builder.Services.AddSingleton<ILogWriter>(sp =>
                new FileLogWriter(
                    options.FilePath,
                    options.MaxFileSizeMB,
                    options.RetainedFileCount));
        }

        // 注册自定义 Provider
        // 注意：不使用 AddProvider，而是直接注册到 ServiceCollection
        // 这样可以确保在所有原生 Provider 之后执行，保持兼容性
        builder.Services.AddSingleton<ILoggerProvider, CustomLoggerProvider>();

        return builder;
    }
}