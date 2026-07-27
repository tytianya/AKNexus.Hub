using AKNexus.Core;
using AKNexus.Core.Logging;
using AKNexus.Core.Utils;
using AKNexus.Framework.Application;
using AKNexus.Framework.Application.Dtos;
using AKNexus.Framework.Infrastructure.Entities;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: AssemblyRegistrar(typeof(ApplicationRegistrar))]
namespace AKNexus.Framework.Application
{
    public sealed class ApplicationRegistrar : IAssemblyRegistrar
    {
        public int Order => 10;
        public void Configure(IKernelBuilder builder)
        {
            StartupLogger.Write(builder.Configuration, "应用组件启动……");
            ConfigureMapper();
            StartupLogger.Write(builder.Configuration, "应用组件启动成功");
        }

        /// <summary>
        /// 配置对象映射
        /// </summary>
        void ConfigureMapper()
        {
            TypeAdapterConfig<User, UserDto>.NewConfig()
                .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");

            TypeAdapterConfig<UserCreateInput, User>.NewConfig();
            TypeAdapterConfig<UserUpdateInput, User>.NewConfig();
        }
    }
}
