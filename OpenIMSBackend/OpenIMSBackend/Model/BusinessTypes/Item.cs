namespace OpenIMSBackend.Core.Model.BusinessTypes
{
    /// <summary>
    /// Represents a physical item (a single thing which is inventoried, for example a drill or a laptop).
    /// <para>
    /// An item has two places: the container which contains it is its target place (where it belongs to) and
    /// <see cref="RealPlace"/> is where it actually is. Both are the same as long as the item is where it belongs.
    /// </para>
    /// </summary>
    public class Item : InventoryObject, IContainee
    {
        /// <summary>The place where the item actually is.</summary>
        public IRealPlace RealPlace { get; set; }

        public Item(string id, string name, string description, IRealPlace realPlace) : base(id, name, description)
        {
            this.RealPlace = realPlace;
        }
    }
}
