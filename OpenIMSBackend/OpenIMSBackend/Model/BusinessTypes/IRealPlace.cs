namespace OpenIMSBackend.Core.Model.BusinessTypes
{
    /// <summary>
    /// Something where an item can actually be (its actual place, in contrast to its target place, which is the
    /// <see cref="IContainer"/> which contains it).
    /// <para>
    /// Currently every <see cref="IContainer"/> (so every area and every container) is a real place. Further kinds
    /// of real places are planned.
    /// </para>
    /// </summary>
    public interface IRealPlace
    {
        /// <summary>The unique identifier of the real place.</summary>
        public string Id { get; }
    }
}
