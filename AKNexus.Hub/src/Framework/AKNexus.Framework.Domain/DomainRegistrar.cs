using AKNexus.Core;
using AKNexus.Core.Log;
using AKNexus.Framework.Domain;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: AssemblyRegistrar(typeof(DomainRegistrar))]
namespace AKNexus.Framework.Domain
{
    public sealed class DomainRegistrar : IAssemblyRegistrar
    {
        public void Configure(IKernelBuilder builder)
        {
            StartupLogger.Write(builder.Configuration, "领域组件启动……");

            StartupLogger.Write(builder.Configuration, "领域组件启动成功");
        }
    }
}
