namespace OpenIMSBackend.Core.Model.PersistenceTypes
{
    /// <summary>
    /// The flat representation of an inventory-object in the persistence.
    /// <para>
    /// The business-types (<see cref="BusinessTypes.Location"/>, <see cref="BusinessTypes.Building"/>, ...) form a
    /// tree of objects. The persistence stores that tree as one record per object which references its parent by
    /// id. Because a record has exactly one parent, an object can never be contained in two places at once.
    /// </para>
    /// </summary>
    public class InventoryObjectRecord
    {
        public string Id { get; set; }
        public InventoryObjectType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// The id of the object which contains this object: the location of a building, the location or building of
        /// an area, the area or container of a container or an item. <see langword="null"/> only for a location.
        /// </summary>
        public string? ParentId { get; set; }

        /// <summary>The id of the place where an item actually is. <see langword="null"/> for every object which is not an item.</summary>
        public string? RealPlaceId { get; set; }

        public InventoryObjectRecord(string id, InventoryObjectType type, string name, string description, string? parentId, string? realPlaceId)
        {
            this.Id = id;
            this.Type = type;
            this.Name = name;
            this.Description = description;
            this.ParentId = parentId;
            this.RealPlaceId = realPlaceId;
        }
    }
}
