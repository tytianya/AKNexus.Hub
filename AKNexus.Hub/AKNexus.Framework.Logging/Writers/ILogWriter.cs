using AKNexus.Framework.Logging.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKNexus.Framework.Logging.Writers
{
    // 日志写入器扩展接口
    public interface ILogWriter
    {
        Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default);
    }
}
