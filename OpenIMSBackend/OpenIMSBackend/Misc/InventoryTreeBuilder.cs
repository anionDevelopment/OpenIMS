using OpenIMSBackend.Core.Model.BusinessTypes;
using OpenIMSBackend.Core.Model.PersistenceTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenIMSBackend.Core.Misc
{
    /// <summary>
    /// Builds the tree of business-types (<see cref="Location"/> -> <see cref="Building"/> -> <see cref="Area"/> -> <see cref="IContainee"/>s)
    /// out of the flat records of the persistence.
    /// <para>
    /// It does not check the rules of the hierarchy: the records are expected to fulfill them, because they can
    /// only be changed through the business-logic which ensures them.
    /// </para>
    /// </summary>
    internal static class InventoryTreeBuilder
    {
        /// <summary>Returns all locations of the given records, each with its complete hierarchy.</summary>
        /// <param name="records">All records of the inventory. A subset is not sufficient, because an item can actually be at a place of another location.</param>
        /// <returns>The locations, ordered by their name. The content of every location and container is ordered by name as well.</returns>
        public static IList<Location> BuildLocations(IEnumerable<InventoryObjectRecord> records)
        {
            List<InventoryObjectRecord> orderedRecords = records.OrderBy(record => record.Name).ThenBy(record => record.Id).ToList();
            Dictionary<string, IInventoryObject> objectsById = new Dictionary<string, IInventoryObject>();

            // Items are created last, because an item references its real place, which therefore has to exist already.
            foreach (InventoryObjectRecord record in orderedRecords.Where(record => record.Type != InventoryObjectType.Item))
            {
                objectsById[record.Id] = CreatePlace(record);
            }
            foreach (InventoryObjectRecord record in orderedRecords.Where(record => record.Type == InventoryObjectType.Item))
            {
                objectsById[record.Id] = new Item(record.Id, record.Name, record.Description, (IRealPlace)objectsById[record.RealPlaceId!]);
            }

            foreach (InventoryObjectRecord record in orderedRecords.Where(record => record.ParentId != null))
            {
                IInventoryObject child = objectsById[record.Id];
                IInventoryObject parent = objectsById[record.ParentId!];
                AddChild(parent, child);
            }

            return orderedRecords.Where(record => record.Type == InventoryObjectType.Location).Select(record => (Location)objectsById[record.Id]).ToList();
        }

        private static void AddChild(IInventoryObject parent, IInventoryObject child)
        {
            switch (parent)
            {
                case Location location when child is Building building:
                    location.Buildings.Add(building);
                    break;
                case Location location:
                    location.Areas.Add((Area)child);
                    break;
                case Building building:
                    building.Areas.Add((Area)child);
                    break;
                default:
                    ((IContainer)parent).Containees.Add((IContainee)child);
                    break;
            }
        }

        private static IInventoryObject CreatePlace(InventoryObjectRecord record)
        {
            return record.Type switch
            {
                InventoryObjectType.Location => new Location(record.Id, record.Name, record.Description),
                InventoryObjectType.Building => new Building(record.Id, record.Name, record.Description),
                InventoryObjectType.Area => new Area(record.Id, record.Name, record.Description),
                InventoryObjectType.Container => new Container(record.Id, record.Name, record.Description),
                _ => throw new ArgumentException($"The record with id '{record.Id}' has the type {record.Type}, which is not a place."),
            };
        }
    }
}
