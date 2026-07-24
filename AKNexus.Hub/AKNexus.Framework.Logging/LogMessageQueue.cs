namespace AKNexus.Framework.Logging;

using AKNexus.Framework.Logging.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Threading.Channels;

//无锁异步通道，用于接收 Logger 产生的数据
public class LogMessageQueue
{
    public Channel<LogEntry> Channel { get; } = System.Threading.Channels.Channel.CreateUnbounded<LogEntry>(
        new UnboundedChannelOptions { SingleReader = true });
    public LogMessageQueue()
    {
        Channel = System.Threading.Channels.Channel.CreateUnbounded<LogEntry>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });
    }
}
