using GRYLibrary.Core.APIServer.BaseServices;

namespace OpenIMSBackend.Core.BackgroundServices
{
    /// <summary>
    /// This service does regularly recurring overhead-work which does not belong to a request, for example the
    /// removal of access-token which have expired.
    /// </summary>
    public interface IManagementScheduler : IIteratingBackgroundService
    {
    }
}
