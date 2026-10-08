namespace OpenIMSBackend.Core.Model.BusinessTypes
{
    /// <summary>
    /// A physical object of the real world which is managed by the inventory (a location, a building, an area, a
    /// container or an item). Every inventory-object is an <see cref="IAuthorizableItem"/>.
    /// </summary>
    public interface IInventoryObject : IAuthorizableItem
    {
        /// <summary>The human-readable name of the object.</summary>
        public string Name { get; set; }

        /// <summary>A free-text-description of the object.</summary>
        public string Description { get; set; }
    }
}
