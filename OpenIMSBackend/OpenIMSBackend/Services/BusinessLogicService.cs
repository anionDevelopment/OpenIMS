using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using Microsoft.AspNetCore.Http;
using OpenIMSBackend.Core.Configuration;
using OpenIMSBackend.Core.Constants;
using OpenIMSBackend.Core.Model.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace OpenIMSBackend.Core.Services
{
    public partial class BusinessLogicService : IBusinessLogicService
    {
        private static readonly object _LockObject = new object();
        private readonly IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> _Configuration;
        private readonly IPersistence _Persistence;
        private readonly IAuthenticationService<Model.BusinessTypes.User> _AuthenticationService;
        private readonly ITimeService _TimeService;
        private readonly IGeneralLogger _Logger;
        private readonly IAuditLog _AuditLog;

        /// <summary>Initializes a new instance of <see cref="BusinessLogicService"/>.</summary>
        /// <param name="persistence">The persistence service used to store and retrieve data.</param>
        /// <param name="authenticationService">The authentication service for user management.</param>
        /// <param name="timeService">The service used to obtain the current time.</param>
        /// <param name="logger">The general-purpose logger.</param>
        /// <param name="configuration">The persisted server configuration.</param>
        /// <param name="auditLog">The audit log service.</param>
        public BusinessLogicService(IPersistence persistence, IAuthenticationService<Model.BusinessTypes.User> authenticationService, ITimeService timeService, IServerLog logger, IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> configuration, IAuditLog auditLog)
        {
            this._Persistence = persistence;
            this._AuthenticationService = authenticationService;
            this._TimeService = timeService;
            this._Logger = logger.Logger;
            this._Configuration = configuration;
            this._AuditLog = auditLog;
        }

        #region Login

        /// <inheritdoc />
        public AccessToken Login(string username, string password)
        {
            if (!this._Configuration.ApplicationSpecificConfiguration.LoginIsEnabled)
            {
                throw new NotAuthorizedException("The login is disabled.");
            }
            AccessToken result;
            try
            {
                result = this._AuthenticationService.Login(username, password);
            }
            catch (BadRequestException exception)
            {
                // The two authentication-services answer a rejected credential with a different status-code (the
                // transient one with 400, the persistent one with 401). The endpoint has to answer the same in
                // every configuration, so the rejection is translated here into the one answer which a rejected
                // credential has: 401. This is a translation and not a fallback - the login still fails.
                throw new BadRequestException(StatusCodes.Status401Unauthorized, exception.Message);
            }
            this._AuditLog.Logger.Log($"User '{username}' logged in.", Microsoft.Extensions.Logging.LogLevel.Information);
            return result;
        }

        /// <inheritdoc />
        public void Logout(string accessToken)
        {
            //the user is resolved before the token is removed, because afterwards it can not be resolved anymore.
            string userId = this._AuthenticationService.GetUserByAccessToken(accessToken).Id;
            this._AuthenticationService.Logout(accessToken);
            this._AuditLog.Logger.Log($"User with id '{userId}' logged out.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <inheritdoc />
        public bool AccessTokenIsValid(string accessToken)
        {
            return this._AuthenticationService.AccessTokenIsValid(accessToken);
        }

        #endregion

        #region User

        /// <inheritdoc />
        public string Register(string username, string password)
        {
            lock (_LockObject)
            {
                if (!this.GetRegistrationIsEnabled())
                {
                    throw new NotAuthorizedException("The creation of a user-account is disabled.");
                }
                if (!CodeUnitSpecificConstants.UserNameRegex().IsMatch(username))
                {
                    throw new BadRequestException($"The username '{username}' is not a valid username.");
                }
                // Reject a name which is already taken. The database enforces this as well (the unique-constraint
                // on Users.Name), but only the check here can answer with a usable error instead of letting a
                // constraint-violation surface as an internal error. It also covers the transient persistence,
                // which has no constraint at all.
                if (this._Persistence.UserWithNameExists(username))
                {
                    throw new BadRequestException($"The username '{username}' is already taken.");
                }
                Model.BusinessTypes.User newUser = Model.BusinessTypes.User.Create(username, this._AuthenticationService.Hash(password), this._TimeService);
                this._AuthenticationService.AddUserTyped(newUser);

                Role userRole = this._AuthenticationService.GetRoleByName(CodeUnitSpecificConstants.RolenameUsers);
                this._AuthenticationService.EnsureUserHasRole(newUser.Id, userRole.Id);

                this._AuditLog.Logger.Log($"User '{username}' with id '{newUser.Id}' registered.", Microsoft.Extensions.Logging.LogLevel.Information);
                return newUser.Id;
            }
        }

        /// <inheritdoc />
        public bool UserWithNameExists(string username)
        {
            return this._Persistence.UserWithNameExists(username);
        }

        /// <inheritdoc />
        public bool UserIsAdministrator(string userId)
        {
            return this._AuthenticationService.GetUser(userId).GetAllRoles().Where(role => role.Name == CodeUnitSpecificConstants.RolenameAdmins).Any();
        }

        /// <inheritdoc />
        public Model.BusinessTypes.User GetUser(string userId)
        {
            return this._AuthenticationService.GetUserTyped(userId);
        }

        /// <inheritdoc />
        public IEnumerable<UserOverviewDTO> GetAllUsersWithRoles(string requesterUserId)
        {
            this.EnsureAdministrator(requesterUserId);
            ISet<Role> allRoles = this._Persistence.GetAllRoles();
            List<UserOverviewDTO> result = new List<UserOverviewDTO>();
            foreach (Model.BusinessTypes.User user in this._Persistence.GetAllUsers().Values)
            {
                ISet<string> roleNames = allRoles.Where(role => this._Persistence.UserHasRole(user.Id, role.Id)).Select(role => role.Name).ToHashSet();
                result.Add(new UserOverviewDTO(user.Id, user.Name, roleNames));
            }
            return result;
        }

        /// <inheritdoc />
        public IEnumerable<string> GetAllRoleNames(string requesterUserId)
        {
            this.EnsureAdministrator(requesterUserId);
            return this._Persistence.GetAllRoles().Select(role => role.Name).ToList();
        }

        /// <inheritdoc />
        public void SetRolesOfUser(string requesterUserId, string targetUserId, ISet<string> roleNames)
        {
            this.EnsureAdministrator(requesterUserId);
            //resolve the requested role-names to roles first (GetRoleByName throws for an unknown role-name, so invalid input is rejected before any change is made).
            ISet<Role> targetRoles = roleNames.Select(this._Persistence.GetRoleByName).ToHashSet();
            foreach (Role role in this._Persistence.GetAllRoles())
            {
                bool shouldHaveRole = targetRoles.Any(targetRole => targetRole.Id == role.Id);
                bool hasRole = this._Persistence.UserHasRole(targetUserId, role.Id);
                if (shouldHaveRole && !hasRole)
                {
                    this._Persistence.AddRoleToUser(targetUserId, role.Id);
                }
                else if (!shouldHaveRole && hasRole)
                {
                    this._Persistence.RemoveRoleFromUser(targetUserId, role.Id);
                }
            }
            this._AuditLog.Logger.Log($"Roles of user '{targetUserId}' set to [{string.Join(", ", roleNames)}] by user '{requesterUserId}'.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        private void EnsureAdministrator(string requesterUserId)
        {
            if (!this.UserIsAdministrator(requesterUserId))
            {
                throw new NotAuthorizedException("Only administrators are allowed to do this.");
            }
        }

        #endregion

        #region Settings

        /// <inheritdoc />
        public bool GetRegistrationIsEnabled()
        {
            // The configuration-value is the default which applies until an administrator decided otherwise, so
            // that there is exactly one value which decides this at runtime: the stored one, if it exists.
            string? storedValue = this._Persistence.GetSetting(CodeUnitSpecificConstants.SettingKeyRegistrationIsEnabled);
            if (storedValue == null || !bool.TryParse(storedValue, out bool result))
            {
                return this._Configuration.ApplicationSpecificConfiguration.RegistrationIsEnabled;
            }
            return result;
        }

        /// <inheritdoc />
        public void SetRegistrationIsEnabled(string requesterUserId, bool enabled)
        {
            this.EnsureAdministrator(requesterUserId);
            this._Persistence.SetSetting(CodeUnitSpecificConstants.SettingKeyRegistrationIsEnabled, enabled.ToString());
            this._AuditLog.Logger.Log($"Setting '{CodeUnitSpecificConstants.SettingKeyRegistrationIsEnabled}' set to '{enabled}' by user '{requesterUserId}'.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <inheritdoc />
        public string GetThemeOfUser(string userId)
        {
            // A value which is not stored (or which is not valid anymore because the set of the accepted values
            // changed) is treated as the default, so that the user-interface never has to deal with an unknown value.
            string? value = this._Persistence.GetUserSetting(userId, CodeUnitSpecificConstants.UserSettingKeyTheme);
            if (value == null || !CodeUnitSpecificConstants.Themes.Contains(value))
            {
                return CodeUnitSpecificConstants.ThemeSystem;
            }
            return value;
        }

        /// <inheritdoc />
        public void SetThemeOfUser(string userId, string theme)
        {
            if (!CodeUnitSpecificConstants.Themes.Contains(theme))
            {
                throw new BadRequestException($"'{theme}' is not a valid color-scheme. Valid are: {string.Join(", ", CodeUnitSpecificConstants.Themes)}.");
            }
            this._Persistence.SetUserSetting(userId, CodeUnitSpecificConstants.UserSettingKeyTheme, theme);
        }

        #endregion
    }
}
