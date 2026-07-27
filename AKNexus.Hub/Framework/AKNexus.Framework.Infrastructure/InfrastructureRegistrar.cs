using AKNexus.Core;
using AKNexus.Framework.Abstractions.Repositories;
using AKNexus.Framework.Abstractions.Services;
using AKNexus.Framework.Abstractions.UnitOfWork;
using AKNexus.Framework.Infrastructure;
using AKNexus.Framework.Infrastructure.Data;
using AKNexus.Framework.Infrastructure.DbContexts;
using AKNexus.Framework.Infrastructure.Repositories;
using AKNexus.Framework.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: AssemblyRegistrar(typeof(InfrastructureRegistrar))]
namespace AKNexus.Framework.Infrastructure
{
    internal class InfrastructureRegistrar : IAssemblyRegistrar
    {
        public int Order => 1;
        public void Configure(IKernelBuilder builder)
        {
            var configuration = builder.Configuration;
            var services = builder.Services;

            // 配置数据库上下文（支持动态切换）
            #region 数据库默认配置
            var databaseConfig = configuration
                .GetSection("DatabaseConfig")
                .Get<DatabaseConfig>()
                ?? new DatabaseConfig();

            var connectionConfig = new DatabaseConnectionConfig
            {
                DatabaseType = databaseConfig.DatabaseType,
                ConnectionStringName = databaseConfig.ConnectionStringName,
                ConnectionString = configuration.GetConnectionString(databaseConfig.ConnectionStringName) ?? string.Empty
            };
            services.AddSingleton(connectionConfig);

            #region 数据上下文
            services.AddDbContext<HubDbContext>((sp, options) =>
                {
                    var connectionConfig = sp.GetRequiredService<DatabaseConnectionConfig>();
                    var connectionString = configuration.GetConnectionString(connectionConfig.ConnectionStringName)
                        ?? connectionConfig.ConnectionString;

                    Action<DbContextOptionsBuilder, DatabaseType, string> optionsAction = (options, dbType, connectionString) =>
                    {
                        
                    };

                    ConfigureDatabaseOptions(options, connectionConfig.DatabaseType, connectionString, optionsAction);
                }); 
            #endregion

            //数据库支持动态切换
            services.AddScoped<IDbContextSwitcher, DbContextSwitcher>();


            // 注册仓储和工作单元
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork<HubDbContext>>();
            #endregion

            #region redis缓存
            var redisConnection = configuration["Redis:ConnectionString"];
            if (!string.IsNullOrEmpty(redisConnection))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = configuration["Redis:InstanceName"];
                });
            }
            #endregion
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
}

