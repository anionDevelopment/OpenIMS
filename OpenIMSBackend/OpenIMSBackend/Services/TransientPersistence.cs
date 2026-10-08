using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.Misc;
using OpenIMSBackend.Core.Model.BusinessTypes;
using OpenIMSBackend.Core.Model.PersistenceTypes;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly IDictionary<string/*id*/, InventoryObjectRecord> _InventoryObjects;
        private readonly HashSet<(string/*roleid*/, string/*authorizableitemid*/)> _RolesOfAuthorizableItems;
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
            this._InventoryObjects = new Dictionary<string, InventoryObjectRecord>();
            this._RolesOfAuthorizableItems = new HashSet<(string, string)>();
        }

        /// <inheritdoc />
        public void Reset()
        {
            lock (_Lock)
            {
                this._Settings.Clear();
                this._UserSettings.Clear();
                this._InventoryObjects.Clear();
                this._RolesOfAuthorizableItems.Clear();
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
            lock (_Lock)
            {
                string roleId = this._TransientAuthenticationServicePersistence.GetRoleByName(roleName).Id;
                this._RolesOfAuthorizableItems.RemoveWhere(assignment => assignment.Item1 == roleId);
                this._TransientAuthenticationServicePersistence.DeleteRoleByName(roleName);
            }
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

        #region Inventory

        /// <inheritdoc />
        public void AddInventoryObject(InventoryObjectRecord inventoryObject)
        {
            lock (_Lock)
            {
                this._InventoryObjects.Add(inventoryObject.Id, Copy(inventoryObject));
            }
        }

        /// <inheritdoc />
        public void UpdateInventoryObject(InventoryObjectRecord inventoryObject)
        {
            lock (_Lock)
            {
                InventoryObjectRecord storedObject = this._InventoryObjects[inventoryObject.Id];
                storedObject.Name = inventoryObject.Name;
                storedObject.Description = inventoryObject.Description;
                storedObject.ParentId = inventoryObject.ParentId;
                storedObject.RealPlaceId = inventoryObject.RealPlaceId;
            }
        }

        /// <inheritdoc />
        public void RemoveInventoryObject(string inventoryObjectId)
        {
            lock (_Lock)
            {
                this._RolesOfAuthorizableItems.RemoveWhere(assignment => assignment.Item2 == inventoryObjectId);
                this._InventoryObjects.Remove(inventoryObjectId);
            }
        }

        /// <inheritdoc />
        public bool InventoryObjectExists(string inventoryObjectId)
        {
            lock (_Lock)
            {
                return this._InventoryObjects.ContainsKey(inventoryObjectId);
            }
        }

        /// <inheritdoc />
        public InventoryObjectRecord GetInventoryObject(string inventoryObjectId)
        {
            lock (_Lock)
            {
                return Copy(this._InventoryObjects[inventoryObjectId]);
            }
        }

        /// <inheritdoc />
        public IList<InventoryObjectRecord> GetAllInventoryObjects()
        {
            lock (_Lock)
            {
                return this._InventoryObjects.Values.Select(Copy).ToList();
            }
        }

        /// <inheritdoc />
        public bool InventoryObjectHasChildren(string inventoryObjectId)
        {
            lock (_Lock)
            {
                return this._InventoryObjects.Values.Any(inventoryObject => inventoryObject.ParentId == inventoryObjectId);
            }
        }

        /// <inheritdoc />
        public bool InventoryObjectIsUsedAsRealPlace(string inventoryObjectId)
        {
            lock (_Lock)
            {
                return this._InventoryObjects.Values.Any(inventoryObject => inventoryObject.RealPlaceId == inventoryObjectId);
            }
        }

        /// <inheritdoc />
        public IList<InventoryObjectRecord> SearchInventoryObjects(string searchText, ISet<InventoryObjectType> types, int maximalAmountOfResults)
        {
            lock (_Lock)
            {
                return this._InventoryObjects.Values
                    .Where(inventoryObject => types.Contains(inventoryObject.Type) && inventoryObject.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(inventoryObject => inventoryObject.Name, StringComparer.Ordinal)
                    .ThenBy(inventoryObject => inventoryObject.Id, StringComparer.Ordinal)
                    .Take(maximalAmountOfResults)
                    .Select(Copy)
                    .ToList();
            }
        }

        /// <summary>
        /// Returns a copy, so that a caller who changes a returned record does not change the stored one without
        /// calling <see cref="UpdateInventoryObject"/>. This is how the database-persistence behaves as well.
        /// </summary>
        private static InventoryObjectRecord Copy(InventoryObjectRecord inventoryObject)
        {
            return new InventoryObjectRecord(inventoryObject.Id, inventoryObject.Type, inventoryObject.Name, inventoryObject.Description, inventoryObject.ParentId, inventoryObject.RealPlaceId);
        }

        #endregion

        #region Authorization of authorizable items

        /// <inheritdoc />
        public void AddRoleToAuthorizableItem(string roleId, string authorizableItemId)
        {
            lock (_Lock)
            {
                if (!this._RolesOfAuthorizableItems.Add((roleId, authorizableItemId)))
                {
                    throw new InvalidOperationException($"The role with id '{roleId}' is already assigned to the authorizable item with id '{authorizableItemId}'.");
                }
            }
        }

        /// <inheritdoc />
        public void RemoveRoleFromAuthorizableItem(string roleId, string authorizableItemId)
        {
            lock (_Lock)
            {
                this._RolesOfAuthorizableItems.Remove((roleId, authorizableItemId));
            }
        }

        /// <inheritdoc />
        public ISet<string> GetRoleIdsOfAuthorizableItem(string authorizableItemId)
        {
            lock (_Lock)
            {
                return this._RolesOfAuthorizableItems.Where(assignment => assignment.Item2 == authorizableItemId).Select(assignment => assignment.Item1).ToHashSet();
            }
        }

        /// <inheritdoc />
        public ISet<string> GetAuthorizableItemIdsOfRole(string roleId)
        {
            lock (_Lock)
            {
                return this._RolesOfAuthorizableItems.Where(assignment => assignment.Item1 == roleId).Select(assignment => assignment.Item2).ToHashSet();
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
