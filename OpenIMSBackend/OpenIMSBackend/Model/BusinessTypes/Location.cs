using System.Collections.Generic;

namespace OpenIMSBackend.Core.Model.BusinessTypes
{
    /// <summary>
    /// Represents a physical location, for example a site or a property. It is the root of the hierarchy and is
    /// neither a container nor a containee: it groups the buildings and the areas (which are not in a building, for
    /// example a yard or a terrace) which belong to it.
    /// </summary>
    public class Location : InventoryObject
    {
        /// <summary>The buildings of this location. A building belongs to exactly one location.</summary>
        public IList<Building> Buildings { get; } = new List<Building>();

        /// <summary>The areas of this location which are not in a building, for example a yard or a terrace.</summary>
        public IList<Area> Areas { get; } = new List<Area>();

        public Location(string id, string name, string description) : base(id, name, description)
        {
        }
    }
}
