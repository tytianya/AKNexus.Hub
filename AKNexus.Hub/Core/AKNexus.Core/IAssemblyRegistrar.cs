using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKNexus.Core
{
    public interface IAssemblyRegistrar
    {
        /// <summary>
        /// 注册顺序，越小越先执行
        /// >1 基础措施 >10 应用层 >100 web层
        /// </summary>
        int Order => 0;
        /// <summary>
        /// 服务配置（注册，基础参数绑定）
        /// </summary>
        /// <param name="builder"></param>
        void Configure(IKernelBuilder builder);
        /// <summary>
        /// 使用功能
        /// </summary>
        /// <param name="app"></param>
        void Use(IKernelApplication app) { }
    }
}
