using System.Collections.Generic;

namespace OpenIMSBackend.Core.Model.BusinessTypes
{
    /// <summary>
    /// Represents a physical building. It always belongs to exactly one <see cref="Location"/> and consists of
    /// areas (its rooms). It is not a container itself: everything in a building is in one of its areas.
    /// </summary>
    public class Building : InventoryObject
    {
        /// <summary>The areas (rooms) of this building. An area belongs to exactly one building or location.</summary>
        public IList<Area> Areas { get; } = new List<Area>();

        public Building(string id, string name, string description) : base(id, name, description)
        {
        }
    }
}
