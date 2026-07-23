using AKNexus.Core;
using AKNexus.Framework.Application;
using AKNexus.Framework.Domain;
using Microsoft.EntityFrameworkCore.Storage;
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
        public int Order => 10;  // 应用层在基础设施之后
        public void Configure(IKernelBuilder builder)
        {
            Console.WriteLine("Application Register");

            builder.Services.AddSingleton<Test>();
        }
    }
}
