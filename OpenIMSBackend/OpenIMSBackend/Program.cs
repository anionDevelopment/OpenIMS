using GRYLibrary.Core.APIServer.CommonRoutes;
using GRYLibrary.Core.APIServer.ExecutionModes;
using GRYLibrary.Core.APIServer.MaintenanceRoutes;
using GRYLibrary.Core.APIServer.Mid.AuthS;
using GRYLibrary.Core.APIServer.Mid.AutS;
using GRYLibrary.Core.APIServer.Mid.Ex;
using GRYLibrary.Core.APIServer.Mid.M05DLog;
using GRYLibrary.Core.APIServer.MidT.Exception;
using GRYLibrary.Core.APIServer.Services.Auth.R;
using GRYLibrary.Core.APIServer.Services.CredH;
using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Services.Init;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.FilePath;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenIMSBackend.Core.BackgroundServices;
using OpenIMSBackend.Core.Configuration;
using OpenIMSBackend.Core.Constants;
using OpenIMSBackend.Core.Misc;
using OpenIMSBackend.Core.Misc.Logger;
using OpenIMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GUtilities = GRYLibrary.Core.Misc.Utilities;
using OpenIMSBackendUtilities = OpenIMSBackend.Core.Misc.Utilities;

namespace OpenIMSBackend.Core
{
    internal class Program
    {
        internal bool ListenOnEveryIP { get; set; } = true;
        internal bool RunAsync { get; set; } = false;
        internal bool IsRunning { get; set; } = false;
        internal IBusinessLogicService? _BusinessLogicService;
        internal IInitializationService<CommandlineParameter>? _InitializationService;
        internal IGRYLog _Log;
        internal IHostApplicationLifetime? _HostApplicationLifetime;
        internal APIServerConfiguration<CodeUnitSpecificConstants, CodeUnitSpecificConfiguration, CommandlineParameter> _Constants;
        internal Action<FunctionalInformation<CodeUnitSpecificConstants, CodeUnitSpecificConfiguration, CommandlineParameter>> SetupMocks { get; set; }
        public Program()
        {
            this._Log = new InitialLog().Logger;
        }
        internal static int Main(string[] commandlineArguments)
        {
            return new Program().MainImplementation(commandlineArguments);
        }
        internal async Task<int> MainImplementationAsync(string[] commandlineArguments)
        {
            return await Task.Run(() => this.MainImplementation(commandlineArguments));
        }
        internal int MainImplementation(string[] commandlineArguments)
        {
            bool runningUsually = false;
            this.IsRunning = true;
            int result = Tools.RunAPIServer<CommandlineParameter, CodeUnitSpecificConstants, CodeUnitSpecificConfiguration>(GeneralConstants.CodeUnitName, GeneralConstants.CodeUnitDescription, Version3.Parse(GeneralConstants.CodeUnitVersion), OpenIMSBackendUtilities.GetEnvironmentTargetType(), GUtilities.GetExecutionMode(commandlineArguments), commandlineArguments, null, (apiServerConfiguration) =>
            {
                apiServerConfiguration.SetInitializationInformationAction = (initializationInformation) =>
                {
                    if (initializationInformation.CommandlineParameter.EnforceVerbose)
                    {
                        _Log.Configuration.AddLogLevel(LogLevel.Debug);
                    }
                    runningUsually = initializationInformation.ApplicationConstants.ExecutionMode is RunProgram;
                    string domain = string.IsNullOrWhiteSpace(initializationInformation.CommandlineParameter.InitialDomain) ? Tools.GetDefaultDomainValue(GeneralConstants.CodeUnitName) : initializationInformation.CommandlineParameter.InitialDomain;
                    initializationInformation.ApplicationConstants.CommonRoutesHostInformation = new DoNotHostCommonRoutes();
                    initializationInformation.ApplicationConstants.HostMaintenanceInformation = new HostMaintenanceRoutes()
                    {
                        ControllerType = typeof(MaintenanceRoutesController)
                    };
                    initializationInformation.ApplicationConstants.ListenOnEveryIP = this.ListenOnEveryIP;
                    initializationInformation.ApplicationConstants.KnownTypes.Add(typeof(CodeUnitSpecificConfiguration));
                    initializationInformation.ApplicationConstants.AuthenticationMiddleware = typeof(AuthSMiddleware);
                    initializationInformation.ApplicationConstants.AuthorizationMiddleware = typeof(AutSRMiddleware);
                    initializationInformation.ApplicationConstants.LoggingMiddleware = typeof(DRequestLoggingMiddleware);
                    initializationInformation.ApplicationConstants.ExceptionManagerMiddleware = typeof(DefaultExceptionHandlerMiddleware);
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.RegistrationIsEnabled = true;
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.LoginIsEnabled = true;
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.ConfigurationForDLoggingMiddleware = new DRequestLoggingConfiguration()
                    {
                        NotLoggedRoutes = new HashSet<string>()
                        {
                            @$"^/API/Other/Resources/APISpecification/*",
                            @$"^/API/Other/Maintenance/Metrics$",
                            @$"^/API/Other/Maintenance/HealthCheck$",
                        },
                        MaximalLengthofRequestBodies = 500,
                        MaximalLengthOfResponseBodies = 500,
                    };
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.ConfigurationForExceptionManagerMiddleware = new ExceptionManagerConfiguration();
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.MaintenanceRoutesInformation = new MaintenanceRoutesInformation()
                    {
                        EnableEndpointAvailabilityCheck = initializationInformation.CommandlineParameter.InitialEnableEndpointAvailabilityCheckValue,
                        EnableEndpointInitializationState = initializationInformation.CommandlineParameter.InitialEnableEndpointInitializationStateValue,
                        EnableEndpointCurrentVersion = initializationInformation.CommandlineParameter.InitialEnableEndpointCurrentVersionValue,
                        EnableEndpointShowAllEndpoints = initializationInformation.CommandlineParameter.InitialEnableEndpointShowAllEndpointsValue,
                        EnableEndpointHealthCheck = initializationInformation.CommandlineParameter.InitialEnableEndpointHealthCheckValue,
                        EnableEndpointMetrics = initializationInformation.CommandlineParameter.InitialEnableEndpointMetricsValue,
                    };
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.AuthenticationConfiguration = new AuthSConfiguration()
                    {
                        //everything which is not listed here requires a valid access-token. The login itself has to be reachable without one, because it is what produces the token.
                        RoutesWhereUnauthenticatedAccessIsAllowed = new HashSet<string>()
                        {
                            @$"^/favicon\.ico$",
                            @$"^/API/Other/Resources/APISpecification/*",
                            @$"^/API/Other/Maintenance/Metrics$",
                            @$"^/API/Other/Maintenance/HealthCheck$",
                            @$"^/API/v{GeneralConstants.CodeUnitMajorVersion}/UserController/Login$",
                            @$"^/API/v{GeneralConstants.CodeUnitMajorVersion}/UserController/TokenIsValid$",
                        },
                    };
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration = new DatabasePersistenceConfiguration()
                    {
                        DatabaseConnectionString = initializationInformation.CommandlineParameter.InitialDatabaseConnectionString ?? "insert your connection-string here",
                        DatabaseType = initializationInformation.CommandlineParameter.InitialDatabaseType ?? "Transient",
                    };
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.AuditLogConfiguration = GRYLogConfiguration.GetCommonConfiguration(AbstractFilePath.FromString("./Audit.log"), true);
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.ManagementSchedulerServiceLogConfiguration = GRYLogConfiguration.GetCommonConfiguration(AbstractFilePath.FromString("./ManagementService.log"), true);
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.MetricsServiceLogConfiguration = GRYLogConfiguration.GetCommonConfiguration(AbstractFilePath.FromString("./MetricsService.log"), true);
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.CommonRoutesInformation = new CommonRoutesInformation()
                    {
                        ContactLink = $"https://information.{domain}/Products/{GeneralConstants.CodeUnitName}/Contact",
                        LicenseLink = $"https://information.{domain}/Products/{GeneralConstants.CodeUnitName}/License",
                        TermsOfServiceLink = $"https://information.{domain}/Products/{GeneralConstants.CodeUnitName}/TermsOfService"
                    };
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.AuthorizationConfiguration = new AutSRConfiguration();
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.HeaderServiceConfiguration = new HeaderServiceConfiguration();
                    initializationInformation.InitialApplicationConfiguration.ServerConfiguration.HostAPISpecificationForInNonDevelopmentEnvironment = true;
                    initializationInformation.InitialApplicationConfiguration.ServerConfiguration.Protocol = new HTTP();
                    initializationInformation.InitialApplicationConfiguration.ServerConfiguration.Domain = domain;
                    initializationInformation.InitialApplicationConfiguration.ServerConfiguration.DevelopmentCertificatePasswordHex = GeneralConstants.DevelopmentCertificatePasswordHex;
                    initializationInformation.InitialApplicationConfiguration.ServerConfiguration.DevelopmentCertificatePFXHex = GeneralConstants.DevelopmentCertificatePFXHex;
                    if (!initializationInformation.CommandlineParameter.RealRun)
                    {
                        GUtilities.EnsureDirectoryDoesNotExist(initializationInformation.ApplicationConstants.BaseFolder);
                        GUtilities.EnsureDirectoryExists(initializationInformation.ApplicationConstants.BaseFolder);
                    }
                };
                apiServerConfiguration.SetFunctionalInformationAction = (functionalInformation) =>
                {
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration);
                    IGRYLog logger = functionalInformation.Logger;

                    AuditLog auditLog = new AuditLog(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.AuditLogConfiguration, functionalInformation.InitializationInformation.ApplicationConstants.GetLogFolder());
                    if (functionalInformation.InitializationInformation.CommandlineParameter.EnforceVerbose)
                    {
                        auditLog.Logger.Configuration.AddLogLevel(LogLevel.Debug);
                    }
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuditLog>(auditLog);

                    ManagementServiceLog managementServiceLog = new ManagementServiceLog(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.ManagementSchedulerServiceLogConfiguration, functionalInformation.InitializationInformation.ApplicationConstants.GetLogFolder());
                    if (functionalInformation.InitializationInformation.CommandlineParameter.EnforceVerbose)
                    {
                        managementServiceLog.Logger.Configuration.AddLogLevel(LogLevel.Debug);
                    }
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<IManagementServiceLog>(managementServiceLog);

                    MetricsServiceLog metricsServiceLog = new MetricsServiceLog(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.MetricsServiceLogConfiguration, functionalInformation.InitializationInformation.ApplicationConstants.GetLogFolder());
                    if (functionalInformation.InitializationInformation.CommandlineParameter.EnforceVerbose)
                    {
                        metricsServiceLog.Logger.Configuration.AddLogLevel(LogLevel.Debug);
                    }
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<IMetricsServiceLog>(metricsServiceLog);

                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<ITimeService, TimeService>();
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<CommandlineParameter>(functionalInformation.InitializationInformation.CommandlineParameter);
                    bool useDatabase = functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration.DatabaseType != null && functionalInformation.InitializationInformation.CommandlineParameter.RealRun && functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration.DatabaseType != "Transient";
                    if (useDatabase)
                    {
                        logger.Log($"Run persistent using database \"{functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration.DatabaseType}\".", LogLevel.Information);
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuthenticationService<Model.BusinessTypes.User>, PersistentAuthenticationService>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuthenticationServicePersistence<Model.BusinessTypes.User>>(sp => sp.GetRequiredService<IPersistence>());
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IPersistence, DatabasePersistence>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IDatabasePersistenceConfiguration>(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration);
                        if (functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration.DatabaseType == "PostgreSQL")
                        {
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<ISQLProvider, SQLProviderPostgreSQL>();
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<IGenericDatabaseInteractor, PostgreSQLDatabaseInteractor>();
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<IOpenIMSDatabaseInteractor, DatabaseInteractorPostgreSQL>();
                        }
                        else if (functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration.DatabaseType == "MariaDB")
                        {
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<ISQLProvider, SQLProviderMariaDB>();
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<IGenericDatabaseInteractor, MariaDBDatabaseInteractor>();
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<IOpenIMSDatabaseInteractor, DatabaseInteractorMariaDB>();
                        }
                        else
                        {
                            throw new NotSupportedException("Database not supported. For a list of supported databases see the documentation.");
                        }
                    }
                    else
                    {
                        logger.Log($"Run transient.", LogLevel.Information);
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IPersistence, TransientPersistence>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuthenticationServiceSettings>(new AuthenticationServiceSettings()
                        {
                            BaseRoleOfAllUser = CodeUnitSpecificConstants.RolenameUsers
                        });
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuthenticationService<Model.BusinessTypes.User>, TransientAuthenticationService<Model.BusinessTypes.User>>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<ITransientAuthenticationServicePersistence<Model.BusinessTypes.User>, TransientAuthenticationServicePersistence<Model.BusinessTypes.User>>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuthenticationServicePersistence<Model.BusinessTypes.User>>(sp => sp.GetRequiredService<ITransientAuthenticationServicePersistence<Model.BusinessTypes.User>>());
                    }
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<IManagementScheduler, ManagementService>();
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<IBusinessLogicService, BusinessLogicService>();
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuthenticationService>(sp => sp.GetRequiredService<IAuthenticationService<Model.BusinessTypes.User>>());
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<IRoleBasedAuthorizationService, StaticRoleBasedUserAuthorizationService<Model.BusinessTypes.User>>();
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<IUserAuthorizationService>(sp => sp.GetRequiredService<IRoleBasedAuthorizationService>());
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuthorizationService>(sp => sp.GetRequiredService<IUserAuthorizationService>());
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<ICredentialsProvider, HeaderService>();
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.HeaderServiceConfiguration);
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.CommonRoutesInformation);
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.MaintenanceRoutesInformation);
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.ConfigurationForLoggingMiddleware);
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.ConfigurationForDLoggingMiddleware);
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.AuthenticationConfiguration);
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.ConfigurationForAuthenticationMiddleware);
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.AuthorizationConfiguration);
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.ConfigurationForAuthorizationMiddleware);
                    if (runningUsually)
                    {
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IInitializationService<CommandlineParameter>, InitializationService>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IInitializationService>(sp => sp.GetRequiredService<IInitializationService<CommandlineParameter>>());
                    }
                    else
                    {
                        //the generic variant has to be registered here as well, because services like the health-check ask for it and therefore could not be constructed otherwise.
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IInitializationService<CommandlineParameter>, NoInitializationService<CommandlineParameter>>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IInitializationService>(sp => sp.GetRequiredService<IInitializationService<CommandlineParameter>>());
                    }
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<IMetricsService, MetricsService>();
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<IHealthCheck, HealthCheck>();
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<IExampleDataCreator, ExampleDataCreator>();
                    this.SetupMocks?.Invoke(functionalInformation);
                };
                apiServerConfiguration.ConfigureWebApplication = (functionalInformationForWebApplication) =>
                {
                    this._Constants = apiServerConfiguration;
                    this._Log = functionalInformationForWebApplication.WebApplication.Services.GetService<IGRYLog>();
                    try
                    {
                        if (runningUsually)
                        {
                            this._BusinessLogicService = functionalInformationForWebApplication.WebApplication.Services.GetService<IBusinessLogicService>();
                            this._HostApplicationLifetime = functionalInformationForWebApplication.WebApplication.Services.GetService<IHostApplicationLifetime>();
                            IManagementScheduler managementScheduler = GUtilities.GetValue(functionalInformationForWebApplication.WebApplication.Services.GetService<IManagementScheduler>());
                            IMetricsService metricsService = GUtilities.GetValue(functionalInformationForWebApplication.WebApplication.Services.GetService<IMetricsService>());
                            functionalInformationForWebApplication.RunAsync = this.RunAsync;
                            functionalInformationForWebApplication.PreRun = () =>
                            {
                                //initialize
                                this._InitializationService = GUtilities.GetValue(functionalInformationForWebApplication.WebApplication.Services.GetService<IInitializationService<CommandlineParameter>>());
                                this._InitializationService.Initialize(apiServerConfiguration.CommandlineParameter);
                                //start background-services
                                metricsService.StartAsync();
                                managementScheduler.StartAsync();
                            };
                            functionalInformationForWebApplication.PostRun = () =>
                            {
                                metricsService.Stop().Wait();
                                managementScheduler.Stop().Wait();
                            };
                        }
                    }
                    catch (Exception e)
                    {
                        this._Log.Log("Fatal exception occurred. Server will be stopped.", e);
                        this.Stop();
                    }
                };
            }, this._Log);
            this.IsRunning = false;
            return result;
        }

        internal void Stop()
        {
            GUtilities.AssertNotNull(this._Constants, nameof(this._Constants)).CancellationTokenSource.Cancel();
            while (this.IsRunning)
            {
                System.Threading.Thread.Sleep(System.TimeSpan.FromMilliseconds(100));
            }
        }
    }
}
