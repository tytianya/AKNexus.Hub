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