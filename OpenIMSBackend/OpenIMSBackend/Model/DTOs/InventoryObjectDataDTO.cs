namespace OpenIMSBackend.Core.Model.DTOs
{
    /// <summary>The data of an inventory-object which a user enters when creating or editing it.</summary>
    public class InventoryObjectDataDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public InventoryObjectDataDTO(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }
    }
}
