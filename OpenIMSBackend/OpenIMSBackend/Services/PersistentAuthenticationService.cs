using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.Crypto;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using OpenIMSBackend.Core.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace OpenIMSBackend.Core.Services
{
    /// <summary>
    /// Represents an authentication-service where the user-data (user, roles, access-token, etc.) is stored persistently.
    /// </summary>
    public class PersistentAuthenticationService : IAuthenticationService<Model.BusinessTypes.User>
    {
        private readonly IAuthenticationServicePersistence<Model.BusinessTypes.User> _AuthentificationPersistence;
        private readonly IGeneralLogger _Logger;
        private readonly IApplicationConstants<CodeUnitSpecificConstants> _Constants;
        private readonly ITimeService _TimeService;

        /// <summary>
        /// Initializes a new instance of <see cref="PersistentAuthenticationService"/>.
        /// </summary>
        /// <param name="timeService">Service used to retrieve the current time.</param>
        /// <param name="authentificationPersistence">Persistence layer for authentication data.</param>
        /// <param name="logger">Logger used for diagnostic output.</param>
        /// <param name="constants">Application-wide constants.</param>
        public PersistentAuthenticationService(ITimeService timeService, IAuthenticationServicePersistence<Model.BusinessTypes.User> authentificationPersistence, IServerLog logger, IApplicationConstants<CodeUnitSpecificConstants> constants)
        {
            this._AuthentificationPersistence = authentificationPersistence;
            this._Logger = logger.Logger;
            this._Constants = constants;
            this._TimeService = timeService;
        }

        /// <summary>
        /// Determines whether the given access token is currently valid.
        /// </summary>
        /// <param name="accessToken">The access token string to validate.</param>
        /// <returns><c>true</c> if the token exists and has not expired; otherwise <c>false</c>.</returns>
        public bool AccessTokenIsValid(string accessToken)
        {
            try
            {
                return this._AuthentificationPersistence.GetAccessToken(accessToken).IsValid(this._TimeService);
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public void AddRole(string roleName)
        {
            Role newRole = new Role()
            {
                Id = Guid.NewGuid().ToString(),
                Name = roleName,
                DirectlyInheritedRoles = new HashSet<Role>(),
            };
            this._AuthentificationPersistence.AddRole(newRole);
        }

        /// <inheritdoc />
        public void AddUser(Model.BusinessTypes.User user)
        {
            this.AddUserTyped(user);
        }

        /// <inheritdoc />
        public void AddUserTyped(Model.BusinessTypes.User user)
        {
            this._AuthentificationPersistence.AddUser(user);
        }

        /// <inheritdoc />
        public void EnsureRoleDoesNotExist(string roleName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void EnsureRoleExists(string roleName)
        {
            if (!this.RoleExists(roleName))
            {
                this.AddRole(roleName);
            }
        }

        /// <inheritdoc />
        public void EnsureUserDoesNotHaveRole(string userId, string roleId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void EnsureUserHasRole(string userId, string roleId)
        {
            if (!this.UserHasRole(userId, roleId))
            {
                this.AddRoleToUser(userId, roleId);
            }
        }

        /// <inheritdoc />
        public void AddRoleToUser(string userId, string roleId)
        {
            this._AuthentificationPersistence.AddRoleToUser(userId, roleId);
        }

        /// <inheritdoc />
        public ISet<Model.BusinessTypes.User> GetAllUser()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ISet<Model.BusinessTypes.User> GetAllUserTyped()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Role GetRoleByName(string roleName)
        {
            return this._AuthentificationPersistence.GetRoleByName(roleName);
        }

        /// <inheritdoc />
        public Model.BusinessTypes.User GetUserTyped(string userId)
        {
            return this._AuthentificationPersistence.GetUserById(userId);
        }

        /// <summary>Computes a SHA-256 hex-string hash of the given password.</summary>
        /// <param name="password">The plain-text password to hash.</param>
        /// <returns>The hex-encoded SHA-256 hash.</returns>
        public string Hash(string password)
        {
            string result = GUtilities.ByteArrayToHexString(new SHA256().Hash(GUtilities.StringToByteArray(password)));
            return result;
        }

        /// <inheritdoc />
        public AccessToken Login(string userName, string password)
        {
            //an unknown user and a wrong password are answered identically on purpose, so that the answer does not reveal whether the username exists.
            if (!this._AuthentificationPersistence.UserWithNameExists(userName))
            {
                return this.ThrowInvalidCredentialsException();
            }

            Model.BusinessTypes.User userByNameTyped = this.GetUserByNameTyped(userName);
            if (this.Hash(password) != userByNameTyped.PasswordHash)
            {
                return this.ThrowInvalidCredentialsException();
            }

            if (userByNameTyped.UserIsLocked)
            {
                throw new NotAuthorizedException("User '" + userName + "' is locked.");
            }

            AccessToken accessToken = new AccessToken();
            accessToken.Value = Guid.NewGuid().ToString();
            accessToken.ExpiredMoment = this._TimeService.GetCurrentLocalTimeAsDateTimeOffset().AddDays(1.0);
            accessToken.OwnerUserId = userByNameTyped.Id;
            userByNameTyped.AccessToken.Add(accessToken);
            this._AuthentificationPersistence.AddAccessToken(accessToken);
            return accessToken;
        }

        private AccessToken ThrowInvalidCredentialsException()
        {
            throw new BadRequestException(401, "Invalid credentials");
        }

        /// <inheritdoc />
        public void Logout(AccessToken accessToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void LogoutEverywhere(string userId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void RemoveUser(string userId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool RoleExists(string roleName)
        {
            return this._AuthentificationPersistence.RoleExists(roleName);
        }

        /// <inheritdoc />
        public bool UserExists(string userId)
        {
            return this._AuthentificationPersistence.UserWithIdExists(userId);
        }

        /// <inheritdoc />
        public bool UserHasRole(string userId, string roleId)
        {
            return this._AuthentificationPersistence.UserHasRole(userId, roleId);
        }

        /// <inheritdoc />
        public bool UserWithIdExists(string userId)
        {
            return this._AuthentificationPersistence.UserWithIdExists(userId);
        }

        /// <inheritdoc />
        public bool UserWithNameExists(string username)
        {
            return this._AuthentificationPersistence.UserWithNameExists(username);
        }

        /// <inheritdoc />
        public void Logout(string accessToken)
        {
            this._AuthentificationPersistence.RemoveAccessToken(accessToken);
        }

        /// <inheritdoc />
        public void Logout(ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool UserExistsByName(string userName)
        {
            return this._AuthentificationPersistence.UserWithNameExists(userName);
        }

        /// <inheritdoc />
        public void UpdateRole(Role role)
        {
            this._AuthentificationPersistence.UpdateRole(role);
        }

        /// <inheritdoc />
        public ISet<Role> GetRoles(Model.BusinessTypes.User user)
        {
            return user.Roles;
        }

        /// <inheritdoc />
        public void UpdateUser(Model.BusinessTypes.User user)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Model.BusinessTypes.User GetUserByNameTyped(string userName)
        {
            return this._AuthentificationPersistence.GetUserByName(userName);
        }

        /// <inheritdoc />
        public Model.BusinessTypes.User GetUserById(string userId)
        {
            return this._AuthentificationPersistence.GetUserById(userId);
        }

        /// <inheritdoc />
        public void AddUser(User user)
        {
            this.AddUser((Model.BusinessTypes.User)user);
        }

        /// <inheritdoc />
        public string GetUserName(string accessToken)
        {
            return this.GetUserByAccessToken(accessToken).Name;
        }

        /// <inheritdoc />
        ISet<User> IAuthenticationService.GetAllUser()
        {
            return this._AuthentificationPersistence.GetAllUsers().Values.Select(user => (User)user).ToHashSet();
        }

        /// <inheritdoc />
        public User GetUser(string userId)
        {
            return this._AuthentificationPersistence.GetUserById(userId);
        }

        /// <inheritdoc />
        public ISet<string> GetRolesOfUser(string userId)
        {
            return this._AuthentificationPersistence.GetUserById(userId).Roles.Select(role => role.Name).ToHashSet();
        }

        /// <inheritdoc />
        public User GetUserByName(string name)
        {
            return this._AuthentificationPersistence.GetUserByName(name);
        }

        /// <inheritdoc />
        public User GetUserByAccessToken(string accessToken)
        {
            return this._AuthentificationPersistence.GetUserByAccessToken(accessToken);
        }

        /// <inheritdoc />
        public string GetBaseRoleOfAllUser()
        {
            return CodeUnitSpecificConstants.RolenameUsers;
        }

        /// <inheritdoc />
        public ClaimsPrincipal GetPrincipal(string accessToken)
        {
            throw new NotImplementedException();
        }
    }
}
