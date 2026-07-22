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
/// 多数据库上下文工厂 - 支持动态切换数据库
/// </summary>
public static class DbContextFactory
{
    /// <summary>
    /// 添加数据库上下文（支持动态切换）
    /// </summary>
    public static IServiceCollection AddDynamicDbContext<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder, DatabaseType, string>? optionsAction = null)
        where TDbContext : DbContext
    {
        services.AddDbContext<TDbContext>((sp, options) =>
        {
            var connectionConfig = sp.GetRequiredService<DatabaseConnectionConfig>();
            var connectionString = configuration.GetConnectionString(connectionConfig.ConnectionStringName)
                ?? connectionConfig.ConnectionString;

            ConfigureDatabaseOptions(options, connectionConfig.DatabaseType, connectionString, optionsAction);
        });

        services.AddScoped<DatabaseConnectionConfig>();
        services.AddScoped<IDbContextSwitcher, DbContextSwitcher>();

        return services;
    }

    /// <summary>
    /// 配置数据库选项
    /// </summary>
    private static void ConfigureDatabaseOptions(
        DbContextOptionsBuilder options,
        DatabaseType databaseType,
        string connectionString,
        Action<DbContextOptionsBuilder, DatabaseType, string>? optionsAction)
    {
        switch (databaseType)
        {
            case DatabaseType.PostgreSQL:
                options.UseNpgsql(connectionString, opt =>
                {
                    opt.EnableRetryOnFailure(3);
                    opt.CommandTimeout(30);
                });
                break;

            case DatabaseType.SqlServer:
                options.UseSqlServer(connectionString, opt =>
                {
                    opt.EnableRetryOnFailure(3);
                    opt.CommandTimeout(30);
                });
                break;

            case DatabaseType.MySql:
                // 需要安装 Pomelo.EntityFrameworkCore.MySql
                // options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                throw new NotSupportedException("MySQL support requires Pomelo.EntityFrameworkCore.MySql package");

            case DatabaseType.Sqlite:
                options.UseSqlite(connectionString);
                break;

            default:
                throw new NotSupportedException($"Database type {databaseType} is not supported");
        }

        optionsAction?.Invoke(options, databaseType, connectionString);
    }
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