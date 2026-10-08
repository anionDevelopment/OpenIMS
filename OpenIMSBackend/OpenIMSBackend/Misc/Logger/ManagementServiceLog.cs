using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.Logging.GRYLogger;

namespace OpenIMSBackend.Core.Misc.Logger
{
    public interface IManagementServiceLog
    {
        public IGRYLog Logger { get; }
    }
    public class ManagementServiceLog : SemanticLogger, IManagementServiceLog
    {
        public IGRYLog Logger => this.Log;
        public ManagementServiceLog(IGRYLogConfiguration config, string? basePath) : base(config, "ManagementSchedulerService", basePath)
        {
        }
    }
}
