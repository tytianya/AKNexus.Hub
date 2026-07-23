

using AKNexus.Core;
using AKNexus.Test;

[assembly: AssemblyRegistrar(typeof(TestRegistrar))]
namespace AKNexus.Test
{
    public sealed class TestRegistrar : IAssemblyRegistrar
    {
        public void Configure(IKernelBuilder builder)
        {
            Console.WriteLine("Redis Register");

            //builder.Services.AddSingleton<TestDomain>();
        }
    }
}
