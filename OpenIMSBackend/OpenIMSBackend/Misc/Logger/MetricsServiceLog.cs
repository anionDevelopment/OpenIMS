using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.Logging.GRYLogger;

namespace OpenIMSBackend.Core.Misc.Logger
{
    public interface IMetricsServiceLog
    {
        public IGRYLog Logger { get; }
    }
    public class MetricsServiceLog : SemanticLogger, IMetricsServiceLog
    {
        public IGRYLog Logger => this.Log;
        public MetricsServiceLog(IGRYLogConfiguration config, string? basePath) : base(config, "MetricsService", basePath)
        {
        }
    }
}
