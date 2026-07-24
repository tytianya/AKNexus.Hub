using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AKNexus.Core
{
    public interface IKernelBuilder
    {
        IServiceCollection Services { get; }

        IConfiguration Configuration { get; }

        IHostEnvironment Environment { get; }
    }

    public sealed class KernelBuilder : IKernelBuilder
    {
        public IServiceCollection Services { get; }

        public IConfiguration Configuration { get; }

        public IHostEnvironment Environment { get; }

        public KernelBuilder(
            IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            Services = services;
            Configuration = configuration;
            Environment = environment;
        }
    }
}
