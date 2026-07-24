using AKNexus.Framework.Web.Extensions;
using AKNexus.Framework.Web.Middlewares;
using AKNexus.Hub.Web.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

// 使用 Furion 的 Inject() 初始化
var builder = WebApplication.CreateBuilder(args).Inject();

//核心：激活组件自主注入
builder.Services.AddAKNexus(builder.Configuration, builder.Environment);

var app = builder.Build();
// 配置中间件
ConfigureMiddleware(app, builder.Configuration);
#region 中间件管道（原生)
// 中间件管道（按顺序）
//app.UseHttpsRedirection();
//app.UseCors("AllowAll");
//app.UseAuthentication();
//app.UseAuthorization();
//app.MapControllers();
//app.MapHealthChecks("/health"); 
#endregion

// 存活检查：K8s 判断是否重启 Pod
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Name == "liveness"
});

// 就绪检查：K8s 判断是否转发流量
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready")
});

// 运行应用
app.Run();

/// <summary>
/// 配置服务
/// </summary>
#pragma warning disable CS8321 // 已声明本地函数，但从未使用过
[Obsolete("配置服务不再使用，组件使用自管理，这样要用（公开）哪些服务，都在自己的范围内掌控，不会变得无序")]
void ConfigureServices(IServiceCollection services, IConfiguration configuration){}
#pragma warning restore CS8321 // 已声明本地函数，但从未使用过

/// <summary>
/// 配置中间件
/// </summary>
void ConfigureMiddleware(WebApplication app, IConfiguration configuration)
{
    // 使用Framework Web中间件（已包含 UseInject）
    app.UseFrameworkWeb();

    // 全局异常处理
    app.UseGlobalException();

    // 开发环境配置
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseHsts();
    }
}