using AKNexus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKNexus.Framework.Web
{
    public class KernelBuilder : IKernelBuilder
    {
        public IServiceCollection Services => Host.Services;
        public IConfiguration Configuration => Host.Configuration;
        public IHostEnvironment Environment => Host.Environment;

        public IHostApplicationBuilder Host { get; }

        public KernelBuilder(IHostApplicationBuilder hostBuilder)
        {
            // 任何实现了通用抽象的 Builder（包括 WebApplicationBuilder）都可以传进来
            Host = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));
        }
    }
}
