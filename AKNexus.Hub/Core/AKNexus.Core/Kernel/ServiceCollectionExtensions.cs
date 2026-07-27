using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AKNexus.Core.Kernel
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAKNexus(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
        {
            var builder = new KernelBuilder(
                services,
                configuration,
                environment);

            foreach (var registrar in ComponentLoader.Load().OrderBy(s => s.Order))
            {
                try
                {
                    registrar.Configure(builder);
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        $"组件注册失败:{registrar.GetType().FullName}",
                        ex);
                }
            }

            return services;
        }
    }

    public static class ComponentLoader
    {
        private static readonly ConcurrentDictionary<Type, Func<IAssemblyRegistrar>> _factoryCache = new();

        public static IEnumerable<IAssemblyRegistrar> Load()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            //提高性能
            foreach (var assembly in assemblies.Where(s => s.FullName != null
            && s.FullName.StartsWith("AKNexus", StringComparison.OrdinalIgnoreCase)))
            {
                var attrs = assembly
                    .GetCustomAttributes<AssemblyRegistrarAttribute>();

                foreach (var attr in attrs)
                {
                    yield return CreateRegistrar(attr.RegistrarType);
                }
            }
        }

        /// <summary>
        /// 使用 Lambda 表达式编译创建 Registrar 实例（高性能）
        /// </summary>
        private static IAssemblyRegistrar CreateRegistrar(Type type)
        {
            var factory = _factoryCache.GetOrAdd(type, t =>
            {
                var ctor = t.GetConstructor(Type.EmptyTypes);
                if (ctor == null)
                    throw new InvalidOperationException($"{t.Name} 需要无参构造函数");

                var @new = Expression.New(ctor);
                var lambda = Expression.Lambda<Func<IAssemblyRegistrar>>(@new);
                return lambda.Compile();
            });

            return factory();
        }
    }
}
