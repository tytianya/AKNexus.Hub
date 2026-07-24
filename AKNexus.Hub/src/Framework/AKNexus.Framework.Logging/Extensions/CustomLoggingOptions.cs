namespace AKNexus.Framework.Logging.Extensions;

/// <summary>
/// 自定义日志配置
/// </summary>
public class CustomLoggingOptions
{
    /// <summary>
    /// 启用控制台输出
    /// </summary>
    public bool EnableConsole { get; set; } = true;

    /// <summary>
    /// 启用文件输出
    /// </summary>
    public bool EnableFile { get; set; } = true;

    /// <summary>
    /// 日志文件路径
    /// </summary>
    public string FilePath { get; set; } = "logs/app.log";

    /// <summary>
    /// 单个文件最大大小（MB）
    /// </summary>
    public int MaxFileSizeMB { get; set; } = 10;

    /// <summary>
    /// 保留文件数量
    /// </summary>
    public int RetainedFileCount { get; set; } = 30;
}