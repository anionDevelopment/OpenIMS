using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using OpenIMSBackend.Core.Constants;
using OpenIMSBackend.Core.Model.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenIMSBackend.Core.Services
{
    /// <summary>
    /// The part of <see cref="BusinessLogicService"/> which decides who has access to which authorizable item.
    /// <para>
    /// A group is a role of the GRYLibrary (<see cref="Role"/>). The access which an assigned group grants is
    /// read-access: its members can see the item and what is in it. Only the direct assignments of groups to
    /// authorizable items are stored. That the access applies to everything which is contained in an item as well is
    /// computed here at runtime by walking up the hierarchy, and is not stored.
    /// </para>
    /// </summary>
    public partial class BusinessLogicService
    {
        private static readonly ISet<string> _GroupsWhichCanNotBeDeleted = new HashSet<string>() { CodeUnitSpecificConstants.RolenameUsers, CodeUnitSpecificConstants.RolenameAdmins };

        /// <inheritdoc />
        public void CreateGroup(string requesterUserId, string groupName)
        {
            lock (_LockObject)
            {
                this.EnsureAdministrator(requesterUserId);
                if (string.IsNullOrWhiteSpace(groupName) || groupName.Trim() != groupName)
                {
                    throw new BadRequestException($"'{groupName}' is not a valid name for a group: it must not be empty and must not start or end with whitespace.");
                }
                if (this._Persistence.RoleExists(groupName))
                {
                    throw new BadRequestException($"A group with name '{groupName}' already exists.");
                }
                this._Persistence.AddRole(new Role() { Id = Guid.NewGuid().ToString(), Name = groupName, DirectlyInheritedRoles = new HashSet<Role>() });
                this._AuditLog.Logger.Log($"Group '{groupName}' created by user '{requesterUserId}'.", Microsoft.Extensions.Logging.LogLevel.Information);
            }
        }

        /// <inheritdoc />
        public void DeleteGroup(string requesterUserId, string groupName)
        {
            lock (_LockObject)
            {
                this.EnsureAdministrator(requesterUserId);
                Role group = this.GetExistingGroup(groupName);
                if (_GroupsWhichCanNotBeDeleted.Contains(groupName))
                {
                    throw new BadRequestException($"The group '{groupName}' is required by OpenIMS and can not be deleted.");
                }
                // A group which another group inherits can not be deleted, because the members of the inheriting
                // group would lose access silently.
                if (this._Persistence.GetAllRoles().Any(role => role.DirectlyInheritedRoles.Any(inheritedRole => inheritedRole.Id == group.Id)))
                {
                    throw new BadRequestException($"The group '{groupName}' is inherited by another group and can not be deleted.");
                }
                this._Persistence.DeleteRoleByName(groupName);
                this._AuditLog.Logger.Log($"Group '{groupName}' deleted by user '{requesterUserId}'.", Microsoft.Extensions.Logging.LogLevel.Information);
            }
        }

        /// <inheritdoc />
        public IList<IdLabelDTO> GetAuthorizableItemsOfGroup(string requesterUserId, string groupName)
        {
            this.EnsureAdministrator(requesterUserId);
            return this._Persistence.GetAuthorizableItemIdsOfRole(this.GetExistingGroup(groupName).Id)
                .Select(this._Persistence.GetInventoryObject)
                .Select(ToIdLabel)
                .OrderBy(item => item.Label, StringComparer.Ordinal)
                .ToList();
        }

        /// <inheritdoc />
        public void AssignGroupToAuthorizableItem(string requesterUserId, string groupName, string authorizableItemId)
        {
            lock (_LockObject)
            {
                this.EnsureAdministrator(requesterUserId);
                Role group = this.GetExistingGroup(groupName);
                this.GetExistingInventoryObject(authorizableItemId);
                if (!this._Persistence.GetRoleIdsOfAuthorizableItem(authorizableItemId).Contains(group.Id))
                {
                    this._Persistence.AddRoleToAuthorizableItem(group.Id, authorizableItemId);
                    this._AuditLog.Logger.Log($"Group '{groupName}' assigned to authorizable item '{authorizableItemId}' by user '{requesterUserId}'.", Microsoft.Extensions.Logging.LogLevel.Information);
                }
            }
        }

        /// <inheritdoc />
        public void RemoveGroupFromAuthorizableItem(string requesterUserId, string groupName, string authorizableItemId)
        {
            lock (_LockObject)
            {
                this.EnsureAdministrator(requesterUserId);
                Role group = this.GetExistingGroup(groupName);
                this.GetExistingInventoryObject(authorizableItemId);
                if (this._Persistence.GetRoleIdsOfAuthorizableItem(authorizableItemId).Contains(group.Id))
                {
                    this._Persistence.RemoveRoleFromAuthorizableItem(group.Id, authorizableItemId);
                    this._AuditLog.Logger.Log($"Group '{groupName}' removed from authorizable item '{authorizableItemId}' by user '{requesterUserId}'.", Microsoft.Extensions.Logging.LogLevel.Information);
                }
            }
        }

        /// <inheritdoc />
        public ISet<string> GetGroupsOfAuthorizableItem(string requesterUserId, string authorizableItemId)
        {
            this.EnsureAdministrator(requesterUserId);
            this.GetExistingInventoryObject(authorizableItemId);
            return this._Persistence.GetRoleIdsOfAuthorizableItem(authorizableItemId).Select(roleId => this._Persistence.GetRoleById(roleId).Name).ToHashSet();
        }

        /// <inheritdoc />
        public bool UserHasAccess(string userId, string authorizableItemId)
        {
            ISet<string> groupIdsOfUser = this._AuthenticationService.GetUser(userId).GetAllRoles().Select(role => role.Id).ToHashSet();
            // The access is inherited from every object which contains the item, so the item itself and all objects
            // above it (up to its location) are checked.
            this.GetExistingInventoryObject(authorizableItemId);
            string? currentId = authorizableItemId;
            while (currentId != null)
            {
                if (this._Persistence.GetRoleIdsOfAuthorizableItem(currentId).Overlaps(groupIdsOfUser))
                {
                    return true;
                }
                currentId = this._Persistence.GetInventoryObject(currentId).ParentId;
            }
            return false;
        }

        private Role GetExistingGroup(string groupName)
        {
            if (!this._Persistence.RoleExists(groupName))
            {
                throw new BadRequestException(StatusCodes.Status404NotFound, $"No group with name '{groupName}' exists.");
            }
            return this._Persistence.GetRoleByName(groupName);
        }
    }
}
