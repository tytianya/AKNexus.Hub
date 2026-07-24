using System.Collections.Concurrent;
using AKNexus.Framework.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace AKNexus.Framework.Logging;

/// <summary>
/// 自定义日志 Provider - 兼容 ASP.NET Core 原生配置
/// </summary>
[ProviderAlias("Custom")]
internal sealed class CustomLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly IEnumerable<ILogWriter> _writers;
    private IExternalScopeProvider? _scopeProvider;
    private readonly ConcurrentDictionary<string, CustomLogger> _loggers = new();

    public CustomLoggerProvider(IEnumerable<ILogWriter> writers)
    {
        _writers = writers;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, 
            name => new CustomLogger(name, _writers, _scopeProvider));
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}