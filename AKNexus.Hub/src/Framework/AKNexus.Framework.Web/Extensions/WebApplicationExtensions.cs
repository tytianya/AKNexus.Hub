using Furion;
using Furion.SpecificationDocument;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Asp.Versioning;
using System.Reflection;

namespace AKNexus.Framework.Web.Extensions;

/// <summary>
/// Web应用扩展方法
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// 配置Furion服务
    /// </summary>
    public static IServiceCollection AddFrameworkWeb(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IServiceCollection>? configure = null)
    {
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
        services.AddHealthChecks();

        // 调用自定义配置
        configure?.Invoke(services);

        return services;
    }

    /// <summary>
    /// 配置Web应用中间件
    /// </summary>
    public static WebApplication UseFrameworkWeb(this WebApplication app)
    {
        // 启用 Furion 中间件（包含 Swagger）
        app.UseInject();

        // CORS
        app.UseCors("AllowAll");

        // HTTPS重定向
        app.UseHttpsRedirection();

        // 路由
        app.UseRouting();

        // 认证授权
        app.UseAuthentication();
        app.UseAuthorization();

        // 健康检查端点
        app.MapHealthChecks("/health");

        // 映射控制器
        app.MapControllers();

        return app;
    }
}