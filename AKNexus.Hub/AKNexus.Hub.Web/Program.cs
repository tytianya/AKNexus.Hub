using AKNexus.Framework.Web;
using AKNexus.Framework.Web.Extensions;
using AKNexus.Framework.Web.Middlewares;
using AKNexus.Hub.Web.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.Kestrel.Core;

AKNexusHost.Run(args,
    // ================= 1. DI 服务与 Kestrel 规则配置 =================
    builder =>
    {
        // 直接使用标准的 Options 模式配置 Kestrel 参数，无需访问 WebHost！
        builder.Services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(30);
            options.Limits.MaxRequestBodySize = null; // 不限上传大小
        });
    },

    // ================= 2. K8s 健康检查管道挂载 =================
    app =>
    {
        // 存活检查
        app.Endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = r => r.Name == "liveness"
        });

        // 就绪检查
        app.Endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("ready")
        });
    }
);