using AKNexus.Core;
using AKNexus.Core.Logging;
using AKNexus.Core.Utils;
using AKNexus.Framework.Logging;
using AKNexus.Framework.Logging.Providers;
using AKNexus.Framework.Logging.Writers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

[assembly: AssemblyRegistrar(typeof(LoggingRegistrar))]
namespace AKNexus.Framework.Logging;

public class LoggingRegistrar : IAssemblyRegistrar
{
    public int Order => 0;

    public void Configure(IKernelBuilder builder)
    {
        StartupLogger.Write(builder.Configuration, "日志组件启动……");
        var services = builder.Services;

        // 1. 绑定自定义配置
        services.Configure<CustomLoggingOptions>(
            builder.Configuration.GetSection("Logging:AKNexusLog"));

        services.AddSingleton<LogMessageQueue>();
        services.AddHostedService<LogProcessorHostedService>();

        services.TryAddEnumerable(
        [
            ServiceDescriptor.Singleton<ILogWriter, ConsoleLogWriter>(),
            ServiceDescriptor.Singleton<ILogWriter, FileLogWriter>()
        ]);

        // ==========================================
        // 核心：精准剔除目标 Provider，而不是 ClearProviders
        // ==========================================

        // 定义需要干掉的官方 Provider 的全限定名
        var providersToRemove = new HashSet<string>
        {
            "Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider",
            //"Microsoft.Extensions.Logging.Debug.DebugLoggerProvider",
            // 如果你确切知道系统内可能存在的原生/第三方 FileLoggerProvider 的类名，也可以加在这里
        };

        // 查找这些 Provider 的服务描述符
        var descriptorsToRemove = services
            .Where(d => d.ServiceType == typeof(ILoggerProvider))
            .Where(d =>
                // 检查按类型注册的
                (d.ImplementationType != null && providersToRemove.Contains(d.ImplementationType.FullName)) ||
                // 检查按实例注册的
                (d.ImplementationInstance != null && providersToRemove.Contains(d.ImplementationInstance.GetType().FullName))
            )
            .ToList();

        // 移除它们
        foreach (var descriptor in descriptorsToRemove)
        {
            services.Remove(descriptor);
        }

        // ==========================================
        // 注册我们自己的 Provider
        // ==========================================
        services.AddLogging(logging =>
        {
            // 直接追加即可。因为我们没用 ClearProviders，
            // 默认的 EventLogLoggerProvider 和 EventSourceLoggerProvider 会自然保留，完美符合你的设计！
            logging.Services.AddSingleton<ILoggerProvider, AKNexusLoggerProvider>();
        });

        StartupLogger.Write(builder.Configuration, "日志组件启动成功");
    }
}