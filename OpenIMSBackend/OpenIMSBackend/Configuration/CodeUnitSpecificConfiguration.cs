using GRYLibrary.Core.APIServer.CommonRoutes;
using GRYLibrary.Core.APIServer.MaintenanceRoutes;
using GRYLibrary.Core.APIServer.Mid.AuthS;
using GRYLibrary.Core.APIServer.Mid.AutS;
using GRYLibrary.Core.APIServer.Mid.M05DLog;
using GRYLibrary.Core.APIServer.MidT.Aut;
using GRYLibrary.Core.APIServer.MidT.Auth;
using GRYLibrary.Core.APIServer.MidT.Exception;
using GRYLibrary.Core.APIServer.MidT.RLog;
using GRYLibrary.Core.APIServer.Services.CredH;
using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.Logging.GRYLogger;

namespace OpenIMSBackend.Core.Configuration
{
    /// <summary>Holds all code-unit-specific runtime configuration values for the OpenIMS backend.</summary>
    public class CodeUnitSpecificConfiguration : ISupportRequestLoggingMiddleware, ISupportExceptionManagerMiddleware, ISupportAuthenticationMiddleware, ISupportAuthorizationMiddleware
    {
        /// <summary>Whether a new user-account can be created at all. When this is <see langword="false"/> then even an administrator can not create an account.</summary>
        public bool RegistrationIsEnabled { get; set; }

        /// <summary>Whether a user can log in at all. Setting this to <see langword="false"/> locks out every user which does not hold a valid access-token yet.</summary>
        public bool LoginIsEnabled { get; set; }

        public IDatabasePersistenceConfiguration DatabasePersistenceConfiguration { get; set; }
        public ICommonRoutesInformation CommonRoutesInformation { get; set; }
        public IMaintenanceRoutesInformation MaintenanceRoutesInformation { get; set; }
        public IDRequestLoggingConfiguration ConfigurationForDLoggingMiddleware { get; set; }
        public IRequestLoggingConfiguration ConfigurationForLoggingMiddleware { get { return this.ConfigurationForDLoggingMiddleware; } }
        public IAutSRConfiguration AuthorizationConfiguration { get; set; }
        public IAuthorizationConfiguration ConfigurationForAuthorizationMiddleware { get { return this.AuthorizationConfiguration; } }
        public IAuthSConfiguration AuthenticationConfiguration { get; set; }
        public IAuthenticationConfiguration ConfigurationForAuthenticationMiddleware { get { return this.AuthenticationConfiguration; } }
        public IExceptionManagerConfiguration ConfigurationForExceptionManagerMiddleware { get; set; }
        public IHeaderServiceConfiguration HeaderServiceConfiguration { get; set; }
        public IGRYLogConfiguration AuditLogConfiguration { get; set; }
        public IGRYLogConfiguration ManagementSchedulerServiceLogConfiguration { get; set; }
        public IGRYLogConfiguration MetricsServiceLogConfiguration { get; set; }
    }
}
