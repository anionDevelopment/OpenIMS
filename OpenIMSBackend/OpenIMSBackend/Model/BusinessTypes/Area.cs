using System.Collections.Generic;

namespace OpenIMSBackend.Core.Model.BusinessTypes
{
    /// <summary>
    /// Represents a physical area where things are kept: a room of a building, or an area of a location which is not
    /// in a building (for example a yard or a terrace). It is in exactly one building or location and is the topmost
    /// container of the hierarchy: it can contain containers and items, but it can not be contained in a container.
    /// </summary>
    public class Area : InventoryObject, IContainer
    {
        /// <inheritdoc />
        public IList<IContainee> Containees { get; } = new List<IContainee>();

        public Area(string id, string name, string description) : base(id, name, description)
        {
        }
    }
}
