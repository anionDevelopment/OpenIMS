using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Utilities;
using OpenIMSBackend.Core.Model.PersistenceTypes;
using System.Collections.Generic;

namespace OpenIMSBackend.Core.Services
{
    /// <summary>
    /// The persistence of OpenIMS.
    /// <para>
    /// Everything which the authentication needs (user, role, access-token) is already required by
    /// <see cref="IAuthenticationServicePersistence{T}"/>, so this interface only adds what belongs to OpenIMS
    /// itself: the settings of the installation, the settings of a single user, the inventory and the few queries
    /// which the metrics- and health-mechanisms use.
    /// </para>
    /// <para>
    /// The persistence only stores the inventory. It does not check the rules of the hierarchy (what may contain
    /// what, no cycles, ...): these are business-rules and are checked by the <see cref="IBusinessLogicService"/>.
    /// </para>
    /// </summary>
    public interface IPersistence : IAuthenticationServicePersistence<Model.BusinessTypes.User>, IReconnectableDatabase
    {
        /// <summary>Returns the total number of user-accounts which currently exist.</summary>
        /// <returns>The count of stored user-accounts.</returns>
        public uint GetAmountOfUsers();

        /// <summary>Returns the value of the setting with the given key, or <see langword="null"/> if the setting is not set.</summary>
        /// <param name="key">The key of the setting.</param>
        /// <returns>The stored value, or <see langword="null"/> if no value is stored for the key.</returns>
        public string? GetSetting(string key);

        /// <summary>Stores (inserts or updates) the value of the setting with the given key.</summary>
        /// <param name="key">The key of the setting.</param>
        /// <param name="value">The value to store.</param>
        public void SetSetting(string key, string value);

        /// <summary>Returns the value of the setting with the given key which belongs to the given user, or <see langword="null"/> if the user did not set it.</summary>
        /// <param name="userId">The id of the user the setting belongs to.</param>
        /// <param name="key">The key of the setting.</param>
        /// <returns>The stored value, or <see langword="null"/> if the user has no value for the key.</returns>
        public string? GetUserSetting(string userId, string key);

        /// <summary>Stores (inserts or updates) the value of the setting with the given key for the given user.</summary>
        /// <param name="userId">The id of the user the setting belongs to.</param>
        /// <param name="key">The key of the setting.</param>
        /// <param name="value">The value to store.</param>
        public void SetUserSetting(string userId, string key, string value);

        #region Inventory

        /// <summary>Stores a new inventory-object.</summary>
        /// <param name="inventoryObject">The object to store. Its id must not be used yet.</param>
        public void AddInventoryObject(InventoryObjectRecord inventoryObject);

        /// <summary>Overwrites the stored inventory-object which has the same id as the given one (name, description, parent and real place; the type can not be changed).</summary>
        /// <param name="inventoryObject">The new state of the object.</param>
        public void UpdateInventoryObject(InventoryObjectRecord inventoryObject);

        /// <summary>Removes the inventory-object with the given id together with the roles which are assigned to it.</summary>
        /// <param name="inventoryObjectId">The id of the object to remove.</param>
        public void RemoveInventoryObject(string inventoryObjectId);

        /// <summary>Returns whether an inventory-object with the given id exists.</summary>
        /// <param name="inventoryObjectId">The id of the object.</param>
        public bool InventoryObjectExists(string inventoryObjectId);

        /// <summary>Returns the inventory-object with the given id.</summary>
        /// <param name="inventoryObjectId">The id of the object.</param>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when no object with the given id exists.</exception>
        public InventoryObjectRecord GetInventoryObject(string inventoryObjectId);

        /// <summary>Returns all stored inventory-objects.</summary>
        public IList<InventoryObjectRecord> GetAllInventoryObjects();

        /// <summary>Returns whether at least one inventory-object has the object with the given id as parent.</summary>
        /// <param name="inventoryObjectId">The id of the possible parent.</param>
        public bool InventoryObjectHasChildren(string inventoryObjectId);

        /// <summary>Returns whether at least one item actually is at the object with the given id.</summary>
        /// <param name="inventoryObjectId">The id of the possible real place.</param>
        public bool InventoryObjectIsUsedAsRealPlace(string inventoryObjectId);

        /// <summary>Returns the inventory-objects of the given types whose name contains the given text (ignoring the case), ordered by name.</summary>
        /// <param name="searchText">The text which the name has to contain. It is taken literally, so it contains no wildcards. An empty text matches every name.</param>
        /// <param name="types">The types of the objects which are searched.</param>
        /// <param name="maximalAmountOfResults">The maximal amount of returned objects.</param>
        public IList<InventoryObjectRecord> SearchInventoryObjects(string searchText, ISet<InventoryObjectType> types, int maximalAmountOfResults);

        #endregion

        #region Authorization of authorizable items

        /// <summary>Assigns the given role (group) to the given authorizable item, so that the members of the role have access to it. The assignment must not exist yet.</summary>
        /// <param name="roleId">The id of the role.</param>
        /// <param name="authorizableItemId">The id of the authorizable item.</param>
        public void AddRoleToAuthorizableItem(string roleId, string authorizableItemId);

        /// <summary>Removes the assignment of the given role (group) to the given authorizable item.</summary>
        /// <param name="roleId">The id of the role.</param>
        /// <param name="authorizableItemId">The id of the authorizable item.</param>
        public void RemoveRoleFromAuthorizableItem(string roleId, string authorizableItemId);

        /// <summary>Returns the ids of the roles (groups) which are directly assigned to the given authorizable item. Access which is inherited from a containing object is not contained.</summary>
        /// <param name="authorizableItemId">The id of the authorizable item.</param>
        public ISet<string> GetRoleIdsOfAuthorizableItem(string authorizableItemId);

        /// <summary>Returns the ids of the authorizable items to which the given role (group) is directly assigned.</summary>
        /// <param name="roleId">The id of the role.</param>
        public ISet<string> GetAuthorizableItemIdsOfRole(string roleId);

        #endregion

        /// <summary>Resets the persistence to its initial empty state.</summary>
        public void Reset();
    }
}
