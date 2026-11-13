using System.Collections.Generic;

namespace OpenIMSBackend.Core.Model.DTOs
{
    /// <summary>One inventory-object of the inventory-tree together with the objects it directly contains.</summary>
    public class InventoryTreeNodeDTO
    {
        public string Id { get; set; }

        /// <summary>The type of the object: "Location", "Building", "Area", "Container" or "Item".</summary>
        public string Type { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>The objects which this object directly contains: the buildings and then the areas of a location, the areas of a building, the containers and items of an area or container. Each of these lists is ordered by the name.</summary>
        public IList<InventoryTreeNodeDTO> Children { get; set; }

        public InventoryTreeNodeDTO(string id, string type, string name, string description, IList<InventoryTreeNodeDTO> children)
        {
            this.Id = id;
            this.Type = type;
            this.Name = name;
            this.Description = description;
            this.Children = children;
        }
    }
}
