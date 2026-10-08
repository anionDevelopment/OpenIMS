using System.Collections.Generic;

namespace OpenIMSBackend.Core.Model.BusinessTypes
{
    /// <summary>
    /// Represents a physical container, for example a cupboard, a shelf or a euro-box. It is contained in an area
    /// or in another container and can contain further containers and items itself.
    /// </summary>
    public class Container : InventoryObject, IContainer, IContainee
    {
        /// <inheritdoc />
        public IList<IContainee> Containees { get; } = new List<IContainee>();

        public Container(string id, string name, string description) : base(id, name, description)
        {
        }
    }
}
