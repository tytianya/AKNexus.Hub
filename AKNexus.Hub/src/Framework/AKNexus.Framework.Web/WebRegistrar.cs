using AKNexus.Core;
using AKNexus.Framework.Abstractions.Services;
using AKNexus.Framework.Application;
using AKNexus.Framework.Infrastructure.Extensions;
using AKNexus.Framework.Web;
using Asp.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: AssemblyRegistrar(typeof(WebRegistrar))]
namespace AKNexus.Framework.Web
{
    internal class WebRegistrar : IAssemblyRegistrar
    {
        public int Order => 100;  // Web 层
        public void Configure(IKernelBuilder builder)
        {
            var configuration = builder.Configuration;
            var services = builder.Services;

            // 添加控制器并启用 Furion 注入
            services.AddControllers()
                    .AddInject();

            // 添加API版本控制
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Api-Version")
                );
            });

            // 配置CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // 添加健康检查
            var healthChecksBuilder = services.AddHealthChecks();

            var databaseConfig = builder.GetDatabaseConfig();
            if (databaseConfig.DatabaseType == Infrastructure.Data.DatabaseType.PostgreSQL)
            {
                healthChecksBuilder.AddNpgSql(databaseConfig.ConnectionString);
            }

            var redisConnection = configuration["Redis:ConnectionString"];
            if (!string.IsNullOrEmpty(redisConnection))
            {
                healthChecksBuilder.AddRedis(redisConnection);
            }
        }
    }
}
