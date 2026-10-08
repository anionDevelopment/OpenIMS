using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using OpenIMSBackend.Core.Model.BusinessTypes;
using OpenIMSBackend.Core.Model.DTOs;
using System.Collections.Generic;

namespace OpenIMSBackend.Core.Services
{
    /// <summary>
    /// The business-logic of OpenIMS. Everything a controller does goes through this interface, so that the
    /// rules (who is allowed to do what, what is a valid value) exist exactly once and are not spread over the
    /// endpoints.
    /// </summary>
    public interface IBusinessLogicService
    {
        #region Login

        /// <summary>Authenticates a user by its credentials.</summary>
        /// <param name="username">The name of the user.</param>
        /// <param name="password">The plain-text password of the user.</param>
        /// <returns>The access-token which the caller has to send in the header of every following authenticated request.</returns>
        /// <exception cref="GRYLibrary.Core.Exceptions.BadRequestException">Thrown with the status-code 401 when the user does not exist or the password is wrong. Both cases are answered identically on purpose, so that the answer does not reveal whether the username exists.</exception>
        /// <exception cref="GRYLibrary.Core.Exceptions.NotAuthorizedException">Thrown when the user exists and the password is correct but the account is locked.</exception>
        public AccessToken Login(string username, string password);

        /// <summary>Invalidates the given access-token, so that it can not be used anymore.</summary>
        /// <param name="accessToken">The access-token to invalidate.</param>
        public void Logout(string accessToken);

        /// <summary>Returns whether the given access-token exists and has not expired yet.</summary>
        /// <param name="accessToken">The access-token to check.</param>
        /// <returns><see langword="true"/> if the token can be used; otherwise <see langword="false"/>.</returns>
        public bool AccessTokenIsValid(string accessToken);

        #endregion

        #region User

        /// <summary>Creates a new user-account which has the role of every user.</summary>
        /// <param name="username">The name of the new account. It must be unique and must match <see cref="Constants.CodeUnitSpecificConstants.UserNameRegex"/>.</param>
        /// <param name="password">The plain-text password of the new account. Only its hash is stored.</param>
        /// <returns>The id of the created user.</returns>
        public string Register(string username, string password);

        /// <summary>Returns whether a user with the given name exists.</summary>
        public bool UserWithNameExists(string username);

        /// <summary>Returns whether the user with the given id has the role which identifies an administrator.</summary>
        public bool UserIsAdministrator(string userId);

        /// <summary>Returns the user with the given id.</summary>
        public User GetUser(string userId);

        /// <summary>Returns all users together with the names of the roles assigned to them. Only an administrator may do this.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        public IEnumerable<UserOverviewDTO> GetAllUsersWithRoles(string requesterUserId);

        /// <summary>Returns the names of all roles which can be assigned to a user. Only an administrator may do this.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        public IEnumerable<string> GetAllRoleNames(string requesterUserId);

        /// <summary>Sets the complete set of roles of the given user (roles not contained are removed, missing ones are added). Only an administrator may do this.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="targetUserId">The id of the user whose roles should be set.</param>
        /// <param name="roleNames">The names of the roles the user should have afterwards.</param>
        public void SetRolesOfUser(string requesterUserId, string targetUserId, ISet<string> roleNames);

        #endregion

        #region Settings

        /// <summary>Returns whether a new user-account can be created at all. This is the value an administrator set; as long as none was set it is the configuration-value.</summary>
        public bool GetRegistrationIsEnabled();

        /// <summary>Sets whether a new user-account can be created at all. Only an administrator may change this.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="enabled">Whether the creation of a new user-account should be possible.</param>
        public void SetRegistrationIsEnabled(string requesterUserId, bool enabled);

        /// <summary>Returns the color-scheme the given user chose.</summary>
        /// <param name="userId">The id of the user.</param>
        /// <returns>"system", "light" or "dark". A user who did not choose a color-scheme yet gets "system", which follows the setting of the operating-system of that user.</returns>
        public string GetThemeOfUser(string userId);

        /// <summary>Sets the color-scheme of the given user.</summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="theme">"system", "light" or "dark".</param>
        public void SetThemeOfUser(string userId, string theme);

        #endregion

        #region Inventory

        // The inventory is a strict hierarchy which represents the physical world:
        // location -> building -> area -> container (any depth) -> item, and location -> area for an area which is
        // not in a building (for example a yard).
        // Every building is in exactly one location, every area is in exactly one location or building, every
        // container and every item is in exactly one area or container, and there are no cycles. Every method here keeps these rules, so they can not be violated.
        // An id which does not exist is answered with a BadRequestException with the status-code 404, a violation
        // of a rule with a BadRequestException with the status-code 400.

        /// <summary>Creates a new location.</summary>
        /// <returns>The id of the created location.</returns>
        public string CreateLocation(string name, string description);

        /// <summary>Creates a new building in the given location.</summary>
        /// <returns>The id of the created building.</returns>
        public string CreateBuilding(string locationId, string name, string description);

        /// <summary>Creates a new area (for example a room of a building or a yard of a location) in the given location or building.</summary>
        /// <param name="parentId">The id of the location or building which contains the new area.</param>
        /// <returns>The id of the created area.</returns>
        public string CreateArea(string parentId, string name, string description);

        /// <summary>Creates a new container in the given area or container.</summary>
        /// <param name="parentContainerId">The id of the area or container which contains the new container.</param>
        /// <returns>The id of the created container.</returns>
        public string CreateContainer(string parentContainerId, string name, string description);

        /// <summary>Creates a new item.</summary>
        /// <param name="parentContainerId">The id of the area or container which contains the new item. This is the place where the item belongs to.</param>
        /// <param name="realPlaceId">The id of the place where the item actually is.</param>
        /// <returns>The id of the created item.</returns>
        public string CreateItem(string parentContainerId, string realPlaceId, string name, string description);

        /// <summary>Changes name and description of the given inventory-object (location, building, area, container or item).</summary>
        public void UpdateInventoryObject(string inventoryObjectId, string name, string description);

        /// <summary>Moves the given building into another location.</summary>
        public void MoveBuilding(string buildingId, string targetLocationId);

        /// <summary>Moves the given area into another location or building.</summary>
        public void MoveArea(string areaId, string targetId);

        /// <summary>Moves the given container or item into another area or container, so that this becomes its new target place. A container can not be moved into itself or into one of the containers it contains.</summary>
        public void MoveContainee(string containeeId, string targetContainerId);

        /// <summary>Sets the place where the given item actually is.</summary>
        public void SetRealPlaceOfItem(string itemId, string realPlaceId);

        /// <summary>Deletes the given inventory-object together with the groups assigned to it. Only an object which contains nothing and where no item actually is can be deleted.</summary>
        public void DeleteInventoryObject(string inventoryObjectId);

        /// <summary>Returns all locations, each with its complete hierarchy.</summary>
        public IList<Location> GetAllLocations();

        /// <summary>Returns the given location with its complete hierarchy.</summary>
        public Location GetLocation(string locationId);

        /// <summary>
        /// Returns the inventory-objects of the given types whose name contains the given text (ignoring the case),
        /// ordered by name and limited to <see cref="BusinessLogicService.MaximalAmountOfSearchResults"/> results. It
        /// is meant for a selection-list which loads its entries while the user types. Only an administrator may do
        /// this, because the result is not restricted to the objects the requesting user has access to.
        /// </summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="searchText">The text which the name has to contain. It is taken literally. An empty text matches every name.</param>
        /// <param name="typeNames">The types of the objects which are searched: "Location", "Building", "Area", "Container" and/or "Item". At least one type is required.</param>
        /// <returns>The id and the name of every found object.</returns>
        public IList<IdLabelDTO> SearchInventoryObjects(string requesterUserId, string searchText, ISet<string> typeNames);

        #endregion

        #region Authorization of authorizable items

        // A group is a role (GRYLibrary.Core.APIServer.CommonDBTypes.Role). Groups can be assigned to authorizable
        // items (currently the inventory-objects); the members of an assigned group have read-access to the item: they
        // can see it and what is in it.
        // Only the direct assignments are stored. The access is inherited at runtime: who has access to an object
        // has access to everything which is contained in it (directly or indirectly) as well.

        /// <summary>Creates a new group (role) which has no members and is assigned to nothing. Only an administrator may do this.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="groupName">The name of the new group. It must not be empty, must not start or end with whitespace and must not be used by another group yet.</param>
        public void CreateGroup(string requesterUserId, string groupName);

        /// <summary>Deletes the given group together with its memberships and its assignments to authorizable items. The groups which OpenIMS needs itself (users and administrators) and a group which another group inherits can not be deleted. Only an administrator may do this.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="groupName">The name of the group.</param>
        public void DeleteGroup(string requesterUserId, string groupName);

        /// <summary>Returns the authorizable items to which the given group is directly assigned (without the items which are only accessible because they are contained in one of them), ordered by their name. Only an administrator may do this.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="groupName">The name of the group.</param>
        /// <returns>The id and the name of every assigned item.</returns>
        public IList<IdLabelDTO> GetAuthorizableItemsOfGroup(string requesterUserId, string groupName);

        /// <summary>Assigns the given group to the given authorizable item. Assigning a group which is already assigned changes nothing. Only an administrator may do this.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="groupName">The name of the group (role).</param>
        /// <param name="authorizableItemId">The id of the authorizable item.</param>
        public void AssignGroupToAuthorizableItem(string requesterUserId, string groupName, string authorizableItemId);

        /// <summary>Removes the assignment of the given group to the given authorizable item. Removing a group which is not assigned changes nothing. Only an administrator may do this.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="groupName">The name of the group (role).</param>
        /// <param name="authorizableItemId">The id of the authorizable item.</param>
        public void RemoveGroupFromAuthorizableItem(string requesterUserId, string groupName, string authorizableItemId);

        /// <summary>Returns the names of the groups which are directly assigned to the given authorizable item (without the groups whose access is inherited from a containing object). Only an administrator may do this.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="authorizableItemId">The id of the authorizable item.</param>
        public ISet<string> GetGroupsOfAuthorizableItem(string requesterUserId, string authorizableItemId);

        /// <summary>Returns whether the given user has (read-)access to the given authorizable item: whether at least one group of the user (including the groups which the groups of the user inherit) is assigned to the item itself or to an object which contains it directly or indirectly.</summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="authorizableItemId">The id of the authorizable item.</param>
        public bool UserHasAccess(string userId, string authorizableItemId);

        #endregion
    }
}
