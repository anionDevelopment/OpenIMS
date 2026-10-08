namespace OpenIMSBackend.Core.Model.BusinessTypes
{
    /// <summary>
    /// Something to which groups (roles) can be assigned, so that the members of these groups have access to it.
    /// <para>
    /// Only the direct assignments are stored. The access to an authorizable item also applies to everything which
    /// is contained in it; this is computed at runtime by the business-logic and is not stored.
    /// </para>
    /// </summary>
    public interface IAuthorizableItem
    {
        /// <summary>The unique identifier of the authorizable item.</summary>
        public string Id { get; }
    }
}
