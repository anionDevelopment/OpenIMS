using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.Misc;
using OpenIMSBackend.Core.Model.BusinessTypes;
using System;
using System.Collections.Generic;

namespace OpenIMSBackend.Core.Services
{
    /// <summary>
    /// An in-memory-persistence. Everything it holds is lost when the backend is stopped, so it is meant for
    /// testcases and for demonstration-purposes and not for productive usage.
    /// <para>
    /// Everything which belongs to the authentication (user, role, access-token) is delegated to the transient
    /// authentication-persistence of the GRYLibrary, so that this class only has to hold what is specific to
    /// OpenIMS.
    /// </para>
    /// </summary>
    public sealed class TransientPersistence : IPersistence
    {
        private readonly IDictionary<string/*key*/, string/*value*/> _Settings;
        private readonly IDictionary<(string/*userid*/, string/*key*/), string/*value*/> _UserSettings;
        private readonly IAuthenticationServicePersistence<User> _TransientAuthenticationServicePersistence;
        private bool _LogConnectionErrors = true;
        private static readonly object _Lock = new object();

        /// <summary>Initializes a new in-memory persistence instance.</summary>
        /// <param name="transientAuthenticationServicePersistence">The in-memory authentication persistence delegate.</param>
        public TransientPersistence(IAuthenticationServicePersistence<User> transientAuthenticationServicePersistence)
        {
            this._TransientAuthenticationServicePersistence = transientAuthenticationServicePersistence;
            this._Settings = new Dictionary<string, string>();
            this._UserSettings = new Dictionary<(string, string), string>();
        }

        /// <inheritdoc />
        public void Reset()
        {
            lock (_Lock)
            {
                this._Settings.Clear();
                this._UserSettings.Clear();
            }
        }

        /// <inheritdoc />
        public (bool, Exception?) IsAvailable()
        {
            return (true, null);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Utilities.NoOperation();
        }

        /// <inheritdoc />
        public uint GetAmountOfUsers()
        {
            return (uint)this._TransientAuthenticationServicePersistence.GetAllUsers().Count;
        }

        #region Users

        /// <inheritdoc />
        public IDictionary<string, User> GetAllUsers()
        {
            return this._TransientAuthenticationServicePersistence.GetAllUsers();
        }

        /// <inheritdoc />
        public void AddUser(User newUser)
        {
            this._TransientAuthenticationServicePersistence.AddUser(newUser);
        }

        /// <inheritdoc />
        public void UpdateUser(User user)
        {
            this._TransientAuthenticationServicePersistence.UpdateUser(user);
        }

        /// <inheritdoc />
        public void RemoveUser(string userId)
        {
            this._TransientAuthenticationServicePersistence.RemoveUser(userId);
        }

        /// <inheritdoc />
        public User GetUserById(string userId)
        {
            return this._TransientAuthenticationServicePersistence.GetUserById(userId);
        }

        /// <inheritdoc />
        public User GetUserByName(string userName)
        {
            return this._TransientAuthenticationServicePersistence.GetUserByName(userName);
        }

        /// <inheritdoc />
        public bool UserWithNameExists(string username)
        {
            return this._TransientAuthenticationServicePersistence.UserWithNameExists(username);
        }

        /// <inheritdoc />
        public bool UserWithIdExists(string userId)
        {
            return this._TransientAuthenticationServicePersistence.UserWithIdExists(userId);
        }

        #endregion

        #region Roles

        /// <inheritdoc />
        public ISet<GRYLibrary.Core.APIServer.CommonDBTypes.Role> GetAllRoles()
        {
            return this._TransientAuthenticationServicePersistence.GetAllRoles();
        }

        /// <inheritdoc />
        public void AddRole(GRYLibrary.Core.APIServer.CommonDBTypes.Role role)
        {
            this._TransientAuthenticationServicePersistence.AddRole(role);
        }

        /// <inheritdoc />
        public void UpdateRole(GRYLibrary.Core.APIServer.CommonDBTypes.Role role)
        {
            this._TransientAuthenticationServicePersistence.UpdateRole(role);
        }

        /// <inheritdoc />
        public void DeleteRoleByName(string roleName)
        {
            this._TransientAuthenticationServicePersistence.DeleteRoleByName(roleName);
        }

        /// <inheritdoc />
        public bool RoleExists(string roleName)
        {
            return this._TransientAuthenticationServicePersistence.RoleExists(roleName);
        }

        /// <inheritdoc />
        public GRYLibrary.Core.APIServer.CommonDBTypes.Role GetRoleByName(string roleName)
        {
            return this._TransientAuthenticationServicePersistence.GetRoleByName(roleName);
        }

        /// <inheritdoc />
        public GRYLibrary.Core.APIServer.CommonDBTypes.Role GetRoleById(string roleId)
        {
            return this._TransientAuthenticationServicePersistence.GetRoleById(roleId);
        }

        /// <inheritdoc />
        public void AddRoleToUser(string userId, string roleId)
        {
            this._TransientAuthenticationServicePersistence.AddRoleToUser(userId, roleId);
        }

        /// <inheritdoc />
        public void RemoveRoleFromUser(string userId, string roleId)
        {
            this._TransientAuthenticationServicePersistence.RemoveRoleFromUser(userId, roleId);
        }

        /// <inheritdoc />
        public bool UserHasRole(string userId, string roleId)
        {
            return this._TransientAuthenticationServicePersistence.UserHasRole(userId, roleId);
        }

        #endregion

        #region AccessToken

        /// <inheritdoc />
        public bool AccessTokenExists(string accessToken, out User? user)
        {
            return this._TransientAuthenticationServicePersistence.AccessTokenExists(accessToken, out user);
        }

        /// <inheritdoc />
        public AccessToken GetAccessToken(string accessToken)
        {
            return this._TransientAuthenticationServicePersistence.GetAccessToken(accessToken);
        }

        /// <inheritdoc />
        public void AddAccessToken(AccessToken newAccessToken)
        {
            this._TransientAuthenticationServicePersistence.AddAccessToken(newAccessToken);
        }

        /// <inheritdoc />
        public void RemoveAccessToken(string accessToken)
        {
            this._TransientAuthenticationServicePersistence.RemoveAccessToken(accessToken);
        }

        /// <inheritdoc />
        public ISet<AccessToken> GetAllAccessTokenOfUser(string userId)
        {
            return this._TransientAuthenticationServicePersistence.GetAllAccessTokenOfUser(userId);
        }

        /// <inheritdoc />
        public User GetUserByAccessToken(string accessToken)
        {
            return this._TransientAuthenticationServicePersistence.GetUserByAccessToken(accessToken);
        }

        #endregion

        #region Settings

        /// <inheritdoc />
        public string? GetSetting(string key)
        {
            lock (_Lock)
            {
                return this._Settings.TryGetValue(key, out string? value) ? value : null;
            }
        }

        /// <inheritdoc />
        public void SetSetting(string key, string value)
        {
            lock (_Lock)
            {
                this._Settings[key] = value;
            }
        }

        /// <inheritdoc />
        public string? GetUserSetting(string userId, string key)
        {
            lock (_Lock)
            {
                return this._UserSettings.TryGetValue((userId, key), out string? value) ? value : null;
            }
        }

        /// <inheritdoc />
        public void SetUserSetting(string userId, string key, string value)
        {
            lock (_Lock)
            {
                this._UserSettings[(userId, key)] = value;
            }
        }

        #endregion

        /// <inheritdoc />
        public void SetLogConnectionAttemptErrors(bool enabled)
        {
            lock (_Lock)
            {
                this._LogConnectionErrors = enabled;
            }
        }

        /// <inheritdoc />
        public void WaitUntilAvailable(TimeSpan timeSpan)
        {
            //the transient persistence is available as soon as it exists, so there is nothing to wait for.
            Utilities.NoOperation();
        }
    }
}
