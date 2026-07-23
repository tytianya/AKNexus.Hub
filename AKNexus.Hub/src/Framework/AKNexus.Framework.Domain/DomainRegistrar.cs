using AKNexus.Core;
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
            Console.WriteLine("Redis Register");

            //builder.Services.AddSingleton<TestDomain>();
        }
    }
}
