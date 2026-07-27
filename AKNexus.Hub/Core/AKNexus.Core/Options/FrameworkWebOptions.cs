using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKNexus.Core.Options
{
    /// <summary>
    /// web服务开启选项
    /// </summary>
    public class FrameworkWebOptions
    {
        /// <summary> 
        /// 是否启用 Furion 底层（含 Swagger） 
        /// </summary>
        public bool UseInject { get; set; } = true;

        /// <summary> 
        /// 是否启用全局异常处理 
        /// </summary>
        public bool UseGlobalException { get; set; } = true;

        /// <summary> 
        /// 是否启用 CORS 跨域 
        /// </summary>
        public bool UseCors { get; set; } = true;
        public string CorsPolicyName { get; set; } = "AllowAll";

        /// <summary> 
        /// 是否启用 HTTPS 重定向 (DAS / K8s 内网环境通常关掉) 
        /// </summary>
        public bool UseHttpsRedirection { get; set; } = false;

        /// <summary> 
        /// 是否启用 JWT / 身份认证 (DAS 设为 false 可彻底移除认证流水线) 
        /// </summary>
        public bool UseAuthentication { get; set; } = true;

        /// <summary> 
        /// 是否启用 授权拦截 
        /// </summary>
        public bool UseAuthorization { get; set; } = true;

        /// <summary> 
        /// 是否自动映射 健康检查 
        /// </summary>
        public bool MapHealthChecks { get; set; } = true;

        /// <summary> 
        /// 是否自动映射 Controller 控制器 
        /// </summary>
        public bool MapControllers { get; set; } = true;
    }
}
