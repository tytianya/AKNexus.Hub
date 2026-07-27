using AKNexus.Framework.Logging.Writers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKNexus.Framework.Logging
{
    // 后台分发日志处理
    public class LogProcessorHostedService : BackgroundService
    {
        private readonly LogMessageQueue _queue;
        private readonly IEnumerable<ILogWriter> _writers;

        public LogProcessorHostedService(LogMessageQueue queue, IEnumerable<ILogWriter> writers)
        {
            _queue = queue;
            _writers = writers;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 持续从队列中读取日志
            await foreach (var entry in _queue.Channel.Reader.ReadAllAsync(stoppingToken))
            {
                foreach (var writer in _writers)
                {
                    try
                    {
                        await writer.WriteAsync(entry, stoppingToken);
                    }
                    catch
                    {
                        // 框架底层容错，防止某一个 Writer（比如 ES 宕机）影响其他 Writer
                    }
                }
            }
        }
    }
}
