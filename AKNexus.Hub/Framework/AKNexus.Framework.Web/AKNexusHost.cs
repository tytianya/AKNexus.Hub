using AKNexus.Core;
using AKNexus.Core.Kernel;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKNexus.Framework.Web
{
    /// <summary>
    /// 核心：host管道
    /// </summary>
    public static class AKNexusHost
    {
        /// <summary>
        /// 一键拉起宿主 (纯生命周期调度)
        /// </summary>
        public static void Run(
            string[] args,
            Action<IKernelBuilder>? configureServices = null,
            Action<IKernelApplication>? configurePipeline = null)
        {
            // 1. 初始化宿主构建器并注入底层基础设施
            var builder = WebApplication.CreateBuilder(args).Inject();
            var kernelBuilder = new KernelBuilder(builder);

            // 2. 自动加载所有组件的服务注册 (Add 阶段，按 Order 升序)
            var registrars = ComponentLoader.Load().OrderBy(r => r.Order).ToList();
            foreach (var registrar in registrars)
            {
                registrar.Configure(kernelBuilder);
            }

            // 3. 执行 Program.cs 传进来的自定义注册 (主程序最后执行，实现覆盖！)
            configureServices?.Invoke(kernelBuilder);

            // 4. 构建 WebApplication
            var app = builder.Build();
            var kernelApp = new KernelApplication(app);

            // 5. 自动加载所有组件的管道与路由挂载 (Use/Map 阶段，按 Order 升序)
            foreach (var registrar in registrars)
            {
                registrar.Use(kernelApp);
            }

            // 6. 执行 Program.cs 传进来的自定义管道挂载
            configurePipeline?.Invoke(kernelApp);

            // 7. 启动宿主
            app.Run();
        }
    }
}
