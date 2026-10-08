namespace OpenIMSBackend.Core.Model.BusinessTypes
{
    /// <summary>
    /// The leaf-role of the composite-pattern: an object which can be contained in an <see cref="IContainer"/>.
    /// <para>
    /// A containee is contained in exactly one container. That container is its target place: the place where
    /// the object belongs to.
    /// </para>
    /// </summary>
    public interface IContainee : IInventoryObject
    {
    }
}
