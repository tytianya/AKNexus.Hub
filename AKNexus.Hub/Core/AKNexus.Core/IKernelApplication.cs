using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKNexus.Core
{
    /// <summary>
    /// 核心管道容器
    /// </summary>
    public interface IKernelApplication
    {
        /// <summary> 
        /// 中间件构建器 (用于 app.App.Use...) 
        /// </summary>
        IApplicationBuilder App { get; }

        /// <summary> 
        /// 路由 API 构建器 (用于 app.Endpoints.Map...) 
        /// </summary>
        IEndpointRouteBuilder Endpoints { get; }

        /// <summary> 
        /// 运行环境 
        /// </summary>
        IHostEnvironment Environment { get; }

        /// <summary> 
        /// DI 容器 
        /// </summary>
        IServiceProvider Services => App.ApplicationServices;
    }
    /// <summary>
    /// 核心宿主包装类 (Adapter 模式)
    /// </summary>
    public class KernelApplication : IKernelApplication
    {
        public IApplicationBuilder App { get; }
        public IEndpointRouteBuilder Endpoints { get; }
        public IHostEnvironment Environment { get; }

        public KernelApplication(WebApplication app)
        {
            App = app ?? throw new ArgumentNullException(nameof(app));
            Endpoints = app;
            Environment = app.Environment;
        }
    }

    public static class KernelApplicationExtensions
    {
        /// <summary> 
        /// 快捷挂载 POST 接口 
        /// </summary>
        public static RouteHandlerBuilder MapPost(this IKernelApplication app, string pattern, Delegate handler)
            => app.Endpoints.MapPost(pattern, handler);

        /// <summary> 
        /// 快捷挂载 GET 接口 
        /// </summary>
        public static RouteHandlerBuilder MapGet(this IKernelApplication app, string pattern, Delegate handler)
            => app.Endpoints.MapGet(pattern, handler);

        /// <summary> 
        /// 快捷挂载路由组 
        /// </summary>
        public static RouteGroupBuilder MapGroup(this IKernelApplication app, string prefix)
            => app.Endpoints.MapGroup(prefix);
    }
}
