using OpenIMSBackend.Core.Model.BusinessTypes;
using OpenIMSBackend.Core.Model.DTOs;
using OpenIMSBackend.Core.Model.PersistenceTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenIMSBackend.Core.Misc
{
    /// <summary>
    /// Maps the tree of business-types to <see cref="InventoryTreeNodeDTO"/>s. The business-types are not
    /// transported directly, because their children are typed as interfaces (so the type of a child would get lost)
    /// and because <see cref="Item.RealPlace"/> references another object of the tree.
    /// </summary>
    internal static class InventoryTreeNodeDTOMapper
    {
        /// <summary>Returns one tree-node per location, each with its complete hierarchy. The order of the objects is kept.</summary>
        public static IList<InventoryTreeNodeDTO> Map(IEnumerable<Location> locations)
        {
            return locations.Select(Map).ToList();
        }

        private static InventoryTreeNodeDTO Map(IInventoryObject inventoryObject)
        {
            return new InventoryTreeNodeDTO(inventoryObject.Id, GetType(inventoryObject).ToString(), inventoryObject.Name, inventoryObject.Description, GetChildren(inventoryObject).Select(Map).ToList());
        }

        private static IEnumerable<IInventoryObject> GetChildren(IInventoryObject inventoryObject)
        {
            return inventoryObject switch
            {
                Location location => location.Buildings.Cast<IInventoryObject>().Concat(location.Areas),
                Building building => building.Areas,
                IContainer container => container.Containees,
                Item => Enumerable.Empty<IInventoryObject>(),
                _ => throw new ArgumentException($"The inventory-object with id '{inventoryObject.Id}' has the unknown type {inventoryObject.GetType().Name}."),
            };
        }

        private static InventoryObjectType GetType(IInventoryObject inventoryObject)
        {
            return inventoryObject switch
            {
                Location => InventoryObjectType.Location,
                Building => InventoryObjectType.Building,
                Area => InventoryObjectType.Area,
                Container => InventoryObjectType.Container,
                Item => InventoryObjectType.Item,
                _ => throw new ArgumentException($"The inventory-object with id '{inventoryObject.Id}' has the unknown type {inventoryObject.GetType().Name}."),
            };
        }
    }
}
