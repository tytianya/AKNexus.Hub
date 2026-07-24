using AKNexus.Core;
using AKNexus.Core.Log;
using AKNexus.Framework.Abstractions.Services;
using AKNexus.Framework.Application;
using AKNexus.Framework.Infrastructure.Extensions;
using AKNexus.Framework.Web;
using AKNexus.Framework.Web.Utils;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: AssemblyRegistrar(typeof(WebRegistrar))]
namespace AKNexus.Framework.Web
{
    internal sealed class WebRegistrar : IAssemblyRegistrar
    {
        public int Order => 100;
        public void Configure(IKernelBuilder builder)
        {
            StartupLogger.Write(builder.Configuration, "web组件启动……");
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

            // 配置JWT认证
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtConfig = configuration.GetSection("JWT");
                var key = Encoding.UTF8.GetBytes(jwtConfig["IssuerSigningKey"] ?? throw new InvalidOperationException("JWT IssuerSigningKey is not configured"));

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = jwtConfig.GetValue<bool>("ValidateIssuerSigningKey"),
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = jwtConfig.GetValue<bool>("ValidateIssuer"),
                    ValidIssuer = jwtConfig["ValidIssuer"],
                    ValidateAudience = jwtConfig.GetValue<bool>("ValidateAudience"),
                    ValidAudience = jwtConfig["ValidAudience"],
                    ValidateLifetime = jwtConfig.GetValue<bool>("ValidateLifetime"),
                    ClockSkew = TimeSpan.FromMinutes(jwtConfig.GetValue<int>("ClockSkew"))
                };
            });

            // 添加健康检查
            var healthChecksBuilder = services.AddHealthChecks()
                 //存活检查（Liveness）- 只看进程是否活着
                 .AddCheck("liveness", () => HealthCheckResult.Healthy("进程存活"))
                 //就绪检查（Readiness）- 看自身负载
                 .AddCheck<SystemHealthCheck>("system", tags: new[] { "ready" });

            #region 外部健康检查
            // TODO: 安装 AspNetCore.HealthChecks.NpgSql 后启用
            //var databaseConfig = builder.GetDatabaseConfig();
            //if (databaseConfig.DatabaseType == Infrastructure.Data.DatabaseType.PostgreSQL)
            //{
            //    healthChecksBuilder.AddNpgSql(databaseConfig.ConnectionString);
            //}

            // TODO: 安装 AspNetCore.HealthChecks.Redis 后启用
            //var redisConnection = configuration["Redis:ConnectionString"];
            //if (!string.IsNullOrEmpty(redisConnection))
            //{
            //    healthChecksBuilder.AddRedis(redisConnection);
            //} 
            #endregion

            StartupLogger.Write(builder.Configuration, "web组件启动成功");
        }
    }
}
