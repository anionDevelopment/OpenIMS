using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using GRYLibrary.Core.APIServer.Services.Init;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.APIServer.Utilities.InitializationStates;
using GRYLibrary.Core.APIServer.Verbs;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using OpenIMSBackend.Core.Configuration;
using OpenIMSBackend.Core.Constants;
using System.Collections.Generic;

namespace OpenIMSBackend.Core.Services
{
    public class InitializationService : IInitializationService<CommandlineParameter>
    {
        private readonly IAuthenticationService<Model.BusinessTypes.User> _AuthenticationService;
        private readonly IBusinessLogicService _BusinessLogicService;
        private readonly IGeneralLogger _GeneralLogger;
        private readonly IExampleDataCreator _ExampleDataCreator;
        private readonly IApplicationConstants<CodeUnitSpecificConstants> _Constants;
        private readonly IPersistence _Persistence;
        private static readonly object _Lock = new object();
        private InitializationState _InitializationState;

        /// <summary>Initializes a new instance of <see cref="InitializationService"/>.</summary>
        /// <param name="authenticationService">The authentication service used for role and user setup.</param>
        /// <param name="businessLogicService">The business logic service used during initialization.</param>
        /// <param name="generalLogger">The logger for diagnostic output.</param>
        /// <param name="constants">Application-wide constants.</param>
        /// <param name="exampleDataCreator">Service that seeds example data in development environments.</param>
        /// <param name="persistence">The persistence service used to wait for DB availability and read state.</param>
        public InitializationService(IAuthenticationService<Model.BusinessTypes.User> authenticationService, IBusinessLogicService businessLogicService, IServerLog generalLogger, IApplicationConstants<CodeUnitSpecificConstants> constants, IExampleDataCreator exampleDataCreator, IPersistence persistence)
        {
            this._AuthenticationService = authenticationService;
            this._BusinessLogicService = businessLogicService;
            this._GeneralLogger = generalLogger.Logger;
            this._Constants = constants;
            this._ExampleDataCreator = exampleDataCreator;
            this._Persistence = persistence;
            this.SetInitializationState(new Uninitialized());
        }

        /// <inheritdoc />
        public void Initialize(CommandlineParameter commandlineParameter)
        {
            try
            {
                this.SetInitializationState(new Initializing());
                this._GeneralLogger.Log("Initialize services...", Microsoft.Extensions.Logging.LogLevel.Information);
                Tools.WaitUntilDatabaseIsAvailable(this._Persistence, this._GeneralLogger);
                if (this._Persistence is IInitializable initializablePersitence)
                {
                    initializablePersitence.Initialize();//this part runs migrations. this is idempotent and can be done on every start.
                    GRYLibrary.Core.Misc.Utilities.AssertCondition(initializablePersitence.InitializationState is Initialized);
                }
                string adminUsername = CodeUnitSpecificConstants.UsernameAdmin;
                if (!this._BusinessLogicService.UserWithNameExists(adminUsername))
                {
                    //this part runs business-logic-initialization which is not idempotent and will therefore only be executed if it was never done before (which is simply checked by the existence of the admin-user)
                    this._AuthenticationService.EnsureRoleExists(CodeUnitSpecificConstants.RolenameUsers);
                    Role usersRole = this._AuthenticationService.GetRoleByName(CodeUnitSpecificConstants.RolenameUsers);

                    this._AuthenticationService.EnsureRoleExists(CodeUnitSpecificConstants.RolenameAdmins);
                    Role adminsRole = this._AuthenticationService.GetRoleByName(CodeUnitSpecificConstants.RolenameAdmins);
                    adminsRole.DirectlyInheritedRoles = new HashSet<Role>();
                    adminsRole.DirectlyInheritedRoles.Add(usersRole);
                    this._AuthenticationService.UpdateRole(adminsRole);

                    string initialAdminPassword = string.IsNullOrWhiteSpace(commandlineParameter.InitialAdminPassword) ? CodeUnitSpecificConstants.UsernameAdmin : commandlineParameter.InitialAdminPassword;
                    string adminUserId = this._BusinessLogicService.Register(adminUsername, initialAdminPassword);
                    this._AuthenticationService.EnsureUserHasRole(adminUserId, adminsRole.Id);

                    if (this._Constants.Environment is Development)
                    {
                        this._ExampleDataCreator.AddExampleData();
                    }
                }
                this._GeneralLogger.Log("Service is initialized.", Microsoft.Extensions.Logging.LogLevel.Information);
                this.SetInitializationState(new Initialized());
            }
            catch (System.Exception e)
            {
                this.SetInitializationState(new InitializationFailed());
                this._GeneralLogger.Log("Error while service-initialization.", e);
            }
        }

        /// <inheritdoc />
        public void InitializeBase(RunServer commandlineParameter)
        {

        }

        /// <inheritdoc />
        public InitializationState GetInitializationState()
        {
            lock (_Lock)
            {
                return this._InitializationState;
            }
        }

        /// <inheritdoc />
        public void SetInitializationState(InitializationState initializationState)
        {
            lock (_Lock)
            {
                this._InitializationState = initializationState;
            }
        }
    }
}
