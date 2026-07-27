using AKNexus.Core;
using AKNexus.Core.Options;
using AKNexus.Framework.Web.Middlewares;
using Asp.Versioning;
using Furion;
using Furion.SpecificationDocument;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    public static IKernelApplication UseFrameworkWeb(this IKernelApplication app,
        Action<FrameworkWebOptions>? configure = null)
    {
        var options = new FrameworkWebOptions();
        configure?.Invoke(options);
        var app2 = app.App;

        // Furion 中间件 (Swagger 等)
        if (options.UseInject) app.App.UseInject();

        // 全局异常处理
        if (options.UseGlobalException) app.App.UseGlobalException();

        // 开发 / 生产环境区别配置
        if (app.Environment.IsDevelopment())
        {
            app.App.UseDeveloperExceptionPage();
        }
        else
        {
            app.App.UseHsts();
        }

        // CORS跨域
        if (options.UseCors) app.App.UseCors(options.CorsPolicyName);

        // HTTPS 重定向
        if (options.UseHttpsRedirection) app.App.UseHttpsRedirection();

        // 路由注册
        app.App.UseRouting();

        // ================= 核心：JWT / 认证授权的热插拔控制 =================
        // 认证授权
        if (options.UseAuthentication) app.App.UseAuthentication();
        if (options.UseAuthorization) app.App.UseAuthorization();

        // 健康检查端点
        if (options.MapHealthChecks) app.Endpoints.MapHealthChecks("/health");

        // 映射控制器
        if (options.MapControllers) app.Endpoints.MapControllers();

        return app;
    }
}