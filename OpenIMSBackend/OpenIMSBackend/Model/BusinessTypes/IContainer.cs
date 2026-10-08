using System.Collections.Generic;

namespace OpenIMSBackend.Core.Model.BusinessTypes
{
    /// <summary>
    /// The composite-role of the composite-pattern: an object which contains <see cref="IContainee"/>s.
    /// <para>
    /// For each of its containees, the container is the target place, so the place where the containee belongs
    /// to. Where a containee actually is can differ from that (see <see cref="Item.RealPlace"/>).
    /// </para>
    /// </summary>
    public interface IContainer : IInventoryObject, IRealPlace
    {
        /// <summary>The objects which are directly contained in this container.</summary>
        public IList<IContainee> Containees { get; }
    }
}
