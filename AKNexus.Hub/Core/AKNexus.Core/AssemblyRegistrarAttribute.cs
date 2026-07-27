using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKNexus.Core
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class AssemblyRegistrarAttribute : Attribute
    {
        public Type RegistrarType { get; }

        public AssemblyRegistrarAttribute(Type registrarType)
        {
            if (!typeof(IAssemblyRegistrar)
            .IsAssignableFrom(registrarType))
            {
                throw new ArgumentException(
                    $"{registrarType.Name}必须实现IAssemblyRegistrar");
            }

            RegistrarType = registrarType;
        }
    }
}
