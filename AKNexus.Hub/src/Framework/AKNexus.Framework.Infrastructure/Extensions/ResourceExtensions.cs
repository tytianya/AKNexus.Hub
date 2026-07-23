using AKNexus.Core;
using AKNexus.Framework.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKNexus.Framework.Infrastructure.Extensions
{
    public static class ResourceExtensions
    {
        /// <summary>
        /// 数据库配置
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static DatabaseConnectionConfig GetDatabaseConfig(
        this IKernelBuilder builder)
        {
            return builder.Services
                .GetRegisteredInstance<DatabaseConnectionConfig>()
                ?? throw new InvalidOperationException(
                    "DatabaseConnectionConfig 未注册");
        }
    }
}
