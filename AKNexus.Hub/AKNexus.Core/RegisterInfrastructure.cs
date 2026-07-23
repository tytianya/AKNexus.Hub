using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKNexus.Core
{
    /// <summary>
    /// 自注册组件的基本交互设定
    /// </summary>
    public static class RegisterInfrastructure
    {
        /// <summary>
        /// 组件间共享的基础类型对象，必须是单例模式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static T? GetRegisteredInstance<T>(
        this IServiceCollection services) where T : class
        {
            var descriptor = services.LastOrDefault(x => x.ServiceType == typeof(T));
            return descriptor?.ImplementationInstance as T;
        }
    }
}
