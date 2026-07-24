# AKNexus.Framework.Logging

替换默认控制台输出，保留系统事件日志的自定义日志框架。

## 设计原则

| Provider | 处理方式 | 说明 |
|----------|----------|------|
| ConsoleLogger | ✅ 替换 | 用自定义 ConsoleLogWriter |
| DebugLogger | ✅ 替换 | 用自定义输出 |
| EventLogLogger | ❌ 保留 | Windows 事件日志不动 |
| EventSourceLogger | ❌ 保留 | ETW 事件不动 |

## 配置示例

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    },
    "EventLog": {
      "LogLevel": {
        "Default": "Warning"
      }
    }
  },
  "CustomLogging": {
    "EnableConsole": true,
    "EnableFile": true,
    "FilePath": "logs/hub.log",
    "MaxFileSizeMB": 10,
    "RetainedFileCount": 30
  }
}
```

## 工作原理

```
ILogger.Log() 调用
    ↓
LoggerFactory（过滤：appsettings.json 的 Logging.LogLevel）
    ↓
┌─────────────────────────────────────┐
│  EventLogLoggerProvider（保留）      │  → Windows 事件查看器
│  CustomLoggerProvider（你的）        │  → ConsoleLogWriter / FileLogWriter
└─────────────────────────────────────┘
```

## 关键代码

```csharp
// LoggingRegistrar.cs
services.AddLogging(logging =>
{
    logging.ClearProviders();  // 清除原有

    if (OperatingSystem.IsWindows())
    {
        logging.AddEventLog();  // 重新添加事件日志
    }

    logging.AddCustomLogging();  // 添加自定义
});
```

## 扩展 Writer

```csharp
// ElasticsearchLogWriter.cs
public class ElasticsearchLogWriter : ILogWriter
{
    public async Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default)
    {
        // 写入 ES
    }
}

// 注册
services.AddSingleton<ILogWriter, ElasticsearchLogWriter>();
```