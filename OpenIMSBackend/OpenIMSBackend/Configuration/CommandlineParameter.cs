using CommandLine;
using GRYLibrary.Core.APIServer.Verbs;

namespace OpenIMSBackend.Core.Configuration
{
    public class CommandlineParameter : RunServer
    {

        [Option(nameof(InitialAdminPassword), Required = false)]
        public string? InitialAdminPassword { get; set; }

        [Option(nameof(InitialDatabaseType), Required = false)]
        public string? InitialDatabaseType { get; set; }

        [Option(nameof(InitialDatabaseConnectionString), Required = false)]
        public string? InitialDatabaseConnectionString { get; set; }

        [Option(nameof(InitialDomain), Required = false)]
        public string? InitialDomain { get; set; }

        [Option(nameof(InitialEnableEndpointAvailabilityCheckValue), Required = false, Default = true)]
        public bool InitialEnableEndpointAvailabilityCheckValue { get; set; }

        [Option(nameof(InitialEnableEndpointInitializationStateValue), Required = false, Default = false)]
        public bool InitialEnableEndpointInitializationStateValue { get; set; }

        [Option(nameof(InitialEnableEndpointCurrentVersionValue), Required = false, Default = false)]
        public bool InitialEnableEndpointCurrentVersionValue { get; set; }

        [Option(nameof(InitialEnableEndpointShowAllEndpointsValue), Required = false, Default = false)]
        public bool InitialEnableEndpointShowAllEndpointsValue { get; set; }

        [Option(nameof(InitialEnableEndpointHealthCheckValue), Required = false, Default = true)]
        public bool InitialEnableEndpointHealthCheckValue { get; set; }

        [Option(nameof(InitialEnableEndpointMetricsValue), Required = false, Default = false)]
        public bool InitialEnableEndpointMetricsValue { get; set; }
    }
}
