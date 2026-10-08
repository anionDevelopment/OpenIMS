namespace OpenIMSBackend.Core.Model.BusinessTypes
{
    /// <summary>Holds the properties which every <see cref="IInventoryObject"/> has.</summary>
    public abstract class InventoryObject : IInventoryObject
    {
        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string Description { get; set; }

        protected InventoryObject(string id, string name, string description)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
        }
    }
}
