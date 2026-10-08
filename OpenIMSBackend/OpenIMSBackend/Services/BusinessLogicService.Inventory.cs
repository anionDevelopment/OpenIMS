using GRYLibrary.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using OpenIMSBackend.Core.Misc;
using OpenIMSBackend.Core.Model.BusinessTypes;
using OpenIMSBackend.Core.Model.DTOs;
using OpenIMSBackend.Core.Model.PersistenceTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenIMSBackend.Core.Services
{
    /// <summary>
    /// The part of <see cref="BusinessLogicService"/> which manages the inventory. All rules of the hierarchy are
    /// checked here (and only here), the persistence only stores the result.
    /// </summary>
    public partial class BusinessLogicService
    {
        private static readonly ISet<InventoryObjectType> _AreaParentTypes = new HashSet<InventoryObjectType>() { InventoryObjectType.Location, InventoryObjectType.Building };
        private static readonly ISet<InventoryObjectType> _ContainerTypes = new HashSet<InventoryObjectType>() { InventoryObjectType.Area, InventoryObjectType.Container };
        private static readonly ISet<InventoryObjectType> _ContaineeTypes = new HashSet<InventoryObjectType>() { InventoryObjectType.Container, InventoryObjectType.Item };
        private static readonly ISet<InventoryObjectType> _RealPlaceTypes = _ContainerTypes;

        /// <summary>The maximal amount of objects which <see cref="SearchInventoryObjects"/> returns, so that a search with a short text does not load the whole inventory.</summary>
        public const int MaximalAmountOfSearchResults = 20;

        /// <inheritdoc />
        public string CreateLocation(string name, string description)
        {
            lock (_LockObject)
            {
                return this.AddInventoryObject(InventoryObjectType.Location, name, description, null, null);
            }
        }

        /// <inheritdoc />
        public string CreateBuilding(string locationId, string name, string description)
        {
            lock (_LockObject)
            {
                this.GetInventoryObjectOfType(locationId, InventoryObjectType.Location);
                return this.AddInventoryObject(InventoryObjectType.Building, name, description, locationId, null);
            }
        }

        /// <inheritdoc />
        public string CreateArea(string parentId, string name, string description)
        {
            lock (_LockObject)
            {
                this.GetInventoryObjectOfType(parentId, _AreaParentTypes);
                return this.AddInventoryObject(InventoryObjectType.Area, name, description, parentId, null);
            }
        }

        /// <inheritdoc />
        public string CreateContainer(string parentContainerId, string name, string description)
        {
            lock (_LockObject)
            {
                this.GetInventoryObjectOfType(parentContainerId, _ContainerTypes);
                return this.AddInventoryObject(InventoryObjectType.Container, name, description, parentContainerId, null);
            }
        }

        /// <inheritdoc />
        public string CreateItem(string parentContainerId, string realPlaceId, string name, string description)
        {
            lock (_LockObject)
            {
                this.GetInventoryObjectOfType(parentContainerId, _ContainerTypes);
                this.GetInventoryObjectOfType(realPlaceId, _RealPlaceTypes);
                return this.AddInventoryObject(InventoryObjectType.Item, name, description, parentContainerId, realPlaceId);
            }
        }

        /// <inheritdoc />
        public void UpdateInventoryObject(string inventoryObjectId, string name, string description)
        {
            lock (_LockObject)
            {
                EnsureNameIsValid(name);
                InventoryObjectRecord inventoryObject = this.GetExistingInventoryObject(inventoryObjectId);
                inventoryObject.Name = name;
                inventoryObject.Description = description;
                this._Persistence.UpdateInventoryObject(inventoryObject);
            }
        }

        /// <inheritdoc />
        public void MoveBuilding(string buildingId, string targetLocationId)
        {
            lock (_LockObject)
            {
                InventoryObjectRecord building = this.GetInventoryObjectOfType(buildingId, InventoryObjectType.Building);
                this.GetInventoryObjectOfType(targetLocationId, InventoryObjectType.Location);
                building.ParentId = targetLocationId;
                this._Persistence.UpdateInventoryObject(building);
            }
        }

        /// <inheritdoc />
        public void MoveArea(string areaId, string targetId)
        {
            lock (_LockObject)
            {
                // No cycle is possible here: an area can only be in a location or a building, and neither of them can be in an area.
                InventoryObjectRecord area = this.GetInventoryObjectOfType(areaId, InventoryObjectType.Area);
                this.GetInventoryObjectOfType(targetId, _AreaParentTypes);
                area.ParentId = targetId;
                this._Persistence.UpdateInventoryObject(area);
            }
        }

        /// <inheritdoc />
        public void MoveContainee(string containeeId, string targetContainerId)
        {
            lock (_LockObject)
            {
                InventoryObjectRecord containee = this.GetInventoryObjectOfType(containeeId, _ContaineeTypes);
                this.GetInventoryObjectOfType(targetContainerId, _ContainerTypes);
                this.EnsureMoveDoesNotCreateCycle(containeeId, targetContainerId);
                containee.ParentId = targetContainerId;
                this._Persistence.UpdateInventoryObject(containee);
            }
        }

        /// <inheritdoc />
        public void SetRealPlaceOfItem(string itemId, string realPlaceId)
        {
            lock (_LockObject)
            {
                InventoryObjectRecord item = this.GetInventoryObjectOfType(itemId, InventoryObjectType.Item);
                this.GetInventoryObjectOfType(realPlaceId, _RealPlaceTypes);
                item.RealPlaceId = realPlaceId;
                this._Persistence.UpdateInventoryObject(item);
            }
        }

        /// <inheritdoc />
        public void DeleteInventoryObject(string inventoryObjectId)
        {
            lock (_LockObject)
            {
                this.GetExistingInventoryObject(inventoryObjectId);
                // Deleting an object which still contains something would either leave its content without a place
                // (which the hierarchy does not allow) or delete the content as well (which would hide a deletion
                // of real objects behind the deletion of one object). So the content has to be moved or deleted first.
                if (this._Persistence.InventoryObjectHasChildren(inventoryObjectId))
                {
                    throw new BadRequestException($"The inventory-object with id '{inventoryObjectId}' can not be deleted because it still contains other objects.");
                }
                if (this._Persistence.InventoryObjectIsUsedAsRealPlace(inventoryObjectId))
                {
                    throw new BadRequestException($"The inventory-object with id '{inventoryObjectId}' can not be deleted because at least one item actually is there.");
                }
                // The groups which are assigned to the object are removed together with it: they are no objects of
                // the real world but only the information who has access to it.
                this._Persistence.RemoveInventoryObject(inventoryObjectId);
            }
        }

        /// <inheritdoc />
        public IList<Location> GetAllLocations()
        {
            return InventoryTreeBuilder.BuildLocations(this._Persistence.GetAllInventoryObjects());
        }

        /// <inheritdoc />
        public Location GetLocation(string locationId)
        {
            this.GetInventoryObjectOfType(locationId, InventoryObjectType.Location);
            // The whole inventory is loaded, because the real place of an item of this location can be in another location.
            return this.GetAllLocations().Single(location => location.Id == locationId);
        }

        /// <inheritdoc />
        public IList<IdLabelDTO> SearchInventoryObjects(string requesterUserId, string searchText, ISet<string> typeNames)
        {
            this.EnsureAdministrator(requesterUserId);
            if (typeNames.Count == 0)
            {
                throw new BadRequestException("At least one type of inventory-objects has to be searched.");
            }
            ISet<InventoryObjectType> types = typeNames.Select(ParseInventoryObjectType).ToHashSet();
            return this._Persistence.SearchInventoryObjects(searchText, types, MaximalAmountOfSearchResults).Select(ToIdLabel).ToList();
        }

        private static InventoryObjectType ParseInventoryObjectType(string typeName)
        {
            if (!Enum.TryParse(typeName, false, out InventoryObjectType result) || !Enum.IsDefined(result))
            {
                throw new BadRequestException($"'{typeName}' is not a type of inventory-objects. Valid are: {string.Join(", ", Enum.GetNames<InventoryObjectType>())}.");
            }
            return result;
        }

        private static IdLabelDTO ToIdLabel(InventoryObjectRecord inventoryObject)
        {
            return new IdLabelDTO(inventoryObject.Id, inventoryObject.Name);
        }

        private string AddInventoryObject(InventoryObjectType type, string name, string description, string? parentId, string? realPlaceId)
        {
            EnsureNameIsValid(name);
            string id = Guid.NewGuid().ToString();
            this._Persistence.AddInventoryObject(new InventoryObjectRecord(id, type, name, description, parentId, realPlaceId));
            return id;
        }

        private static void EnsureNameIsValid(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new BadRequestException("The name of an inventory-object must not be empty.");
            }
        }

        /// <summary>
        /// Ensures that the containee is neither the target itself nor one of the objects which contain the target,
        /// because then the containee would (indirectly) contain itself after the move.
        /// </summary>
        private void EnsureMoveDoesNotCreateCycle(string containeeId, string targetContainerId)
        {
            string? currentId = targetContainerId;
            while (currentId != null)
            {
                if (currentId == containeeId)
                {
                    throw new BadRequestException($"The inventory-object with id '{containeeId}' can not be moved into the inventory-object with id '{targetContainerId}' because the target is the object itself or is contained in it.");
                }
                currentId = this._Persistence.GetInventoryObject(currentId).ParentId;
            }
        }

        private InventoryObjectRecord GetInventoryObjectOfType(string inventoryObjectId, InventoryObjectType expectedType)
        {
            return this.GetInventoryObjectOfType(inventoryObjectId, new HashSet<InventoryObjectType>() { expectedType });
        }

        private InventoryObjectRecord GetInventoryObjectOfType(string inventoryObjectId, ISet<InventoryObjectType> expectedTypes)
        {
            InventoryObjectRecord result = this.GetExistingInventoryObject(inventoryObjectId);
            if (!expectedTypes.Contains(result.Type))
            {
                throw new BadRequestException($"The inventory-object with id '{inventoryObjectId}' is a {result.Type} but must be one of: {string.Join(", ", expectedTypes)}.");
            }
            return result;
        }

        private InventoryObjectRecord GetExistingInventoryObject(string inventoryObjectId)
        {
            if (!this._Persistence.InventoryObjectExists(inventoryObjectId))
            {
                throw new BadRequestException(StatusCodes.Status404NotFound, $"No inventory-object with id '{inventoryObjectId}' exists.");
            }
            return this._Persistence.GetInventoryObject(inventoryObjectId);
        }
    }
}
