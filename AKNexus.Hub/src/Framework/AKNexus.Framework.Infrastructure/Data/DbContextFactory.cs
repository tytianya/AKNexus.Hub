using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace AKNexus.Framework.Infrastructure.Data;

/// <summary>
/// 数据库类型枚举
/// </summary>
public enum DatabaseType
{
    PostgreSQL,
    SqlServer,
    MySql,
    Sqlite
}

/// <summary>
/// 数据库默认配置
/// </summary>
public class DatabaseConfig
{
    public DatabaseType DatabaseType
    {
        get
        {
            var databaseType = ConnectionStringName switch
            {
                "Default" => DatabaseType.PostgreSQL,
                "SqlServer" => DatabaseType.SqlServer,
                "MySql" => DatabaseType.MySql,
                "Sqlite" => DatabaseType.Sqlite,
                _ => DatabaseType.PostgreSQL,
            };

            return databaseType;
        }
    }
    /// <summary>
    /// 默认连接数据库名称
    /// </summary>
    public string ConnectionStringName { get; set; } = "Default";
}

/// <summary>
/// 数据库连接配置
/// </summary>
public class DatabaseConnectionConfig
{
    /// <summary>
    /// 数据库类型
    /// </summary>
    public DatabaseType DatabaseType { get; set; } = DatabaseType.PostgreSQL;

    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 连接字符串名称（从配置读取）
    /// </summary>
    public string ConnectionStringName { get; set; } = "Default";
}


/// <summary>
/// 数据库上下文切换器接口
/// </summary>
public interface IDbContextSwitcher
{
    /// <summary>
    /// 切换数据库类型
    /// </summary>
    Task SwitchDatabaseAsync(DatabaseType databaseType, string connectionString);

    /// <summary>
    /// 获取当前数据库类型
    /// </summary>
    DatabaseType GetCurrentDatabaseType();
}

/// <summary>
/// 数据库上下文切换器实现
/// </summary>
internal class DbContextSwitcher : IDbContextSwitcher
{
    private readonly DatabaseConnectionConfig _config;

    public DbContextSwitcher(DatabaseConnectionConfig config)
    {
        _config = config;
    }

    public Task SwitchDatabaseAsync(DatabaseType databaseType, string connectionString)
    {
        _config.DatabaseType = databaseType;
        _config.ConnectionString = connectionString;
        return Task.CompletedTask;
    }

    public DatabaseType GetCurrentDatabaseType()
    {
        return _config.DatabaseType;
    }
}