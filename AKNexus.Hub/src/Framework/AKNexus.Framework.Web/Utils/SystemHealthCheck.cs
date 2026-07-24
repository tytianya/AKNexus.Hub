using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKNexus.Framework.Web.Utils
{
    /// <summary>
    /// 系统健康检查
    /// </summary>
    public class SystemHealthCheck : IHealthCheck
    {
        private static readonly double _cpuThreshold = 80.0;  // CPU 阈值
        private static readonly long _memoryThreshold = 2L * 1024 * 1024 * 1024;  // 2GB
        private static double _lastCpuUsage = 0;
        private static DateTime _lastCheckTime = DateTime.UtcNow;
        private static readonly object _lock = new();

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var (cpuUsage, memoryUsage) = GetSystemMetrics();

                var data = new Dictionary<string, object>
                {
                    ["CpuUsage"] = $"{cpuUsage:F2}%",
                    ["MemoryUsage"] = $"{memoryUsage / 1024 / 1024:F2}MB",
                    ["CpuThreshold"] = $"{_cpuThreshold}%",
                    ["MemoryThreshold"] = $"{_memoryThreshold / 1024 / 1024 / 1024}GB"
                };

                // CPU 或内存超限
                if (cpuUsage > _cpuThreshold || memoryUsage > _memoryThreshold)
                {
                    return Task.FromResult(HealthCheckResult.Degraded(
                        $"系统负载高: CPU={cpuUsage:F2}%, Memory={memoryUsage / 1024 / 1024:F2}MB",
                        null,
                        data));
                }

                return Task.FromResult(HealthCheckResult.Healthy("系统正常", data));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy($"健康检查失败: {ex.Message}"));
            }
        }

        private (double CpuUsage, long MemoryUsage) GetSystemMetrics()
        {
            // 内存使用
            var memoryUsage = GC.GetTotalMemory(forceFullCollection: false);

            // CPU 使用率（跨平台）
            var cpuUsage = GetCpuUsage();

            return (cpuUsage, memoryUsage);
        }

        private double GetCpuUsage()
        {
            lock (_lock)
            {
                var currentTime = DateTime.UtcNow;
                var elapsedTime = (currentTime - _lastCheckTime).TotalSeconds;

                if (elapsedTime < 1.0)  // 少于 1 秒，返回上次结果
                    return _lastCpuUsage;

                var process = Process.GetCurrentProcess();
                var currentCpuTime = process.TotalProcessorTime.TotalSeconds;
                var cpuUsage = (currentCpuTime - _lastCpuTime) / elapsedTime / Environment.ProcessorCount * 100;

                _lastCpuTime = currentCpuTime;
                _lastCheckTime = currentTime;
                _lastCpuUsage = Math.Min(100, Math.Max(0, cpuUsage));

                return _lastCpuUsage;
            }
        }

        private static double _lastCpuTime = 0;
    }
}
