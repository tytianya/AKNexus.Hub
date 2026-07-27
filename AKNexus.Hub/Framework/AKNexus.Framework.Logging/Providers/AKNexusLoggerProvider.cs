using Microsoft.Extensions.Logging;
namespace AKNexus.Framework.Logging.Providers
{
    // 关键点：这个别名对应 appsettings.json 里的 Logging:AKNexusLog 节点
    [ProviderAlias("AKNexusLog")]
    public class AKNexusLoggerProvider : ILoggerProvider
    {
        private readonly LogMessageQueue _queue;

        public AKNexusLoggerProvider(LogMessageQueue queue)
        {
            _queue = queue;
        }

        public ILogger CreateLogger(string categoryName) => new AKNexusLogger(categoryName, _queue);

        public void Dispose() { }
    }
}
