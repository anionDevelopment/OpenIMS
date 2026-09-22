using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Utilities;

namespace OpenIMSBackend.Core.Services
{
    /// <summary>
    /// The persistence of OpenIMS.
    /// <para>
    /// Everything which the authentication needs (user, role, access-token) is already required by
    /// <see cref="IAuthenticationServicePersistence{T}"/>, so this interface only adds what belongs to OpenIMS
    /// itself: the settings of the installation, the settings of a single user and the few queries which the
    /// metrics- and health-mechanisms use.
    /// </para>
    /// </summary>
    public interface IPersistence : IAuthenticationServicePersistence<Model.BusinessTypes.User>, IReconnectableDatabase
    {
        /// <summary>Returns the total number of user-accounts which currently exist.</summary>
        /// <returns>The count of stored user-accounts.</returns>
        public uint GetAmountOfUsers();

        /// <summary>Returns the value of the setting with the given key, or <see langword="null"/> if the setting is not set.</summary>
        /// <param name="key">The key of the setting.</param>
        /// <returns>The stored value, or <see langword="null"/> if no value is stored for the key.</returns>
        public string? GetSetting(string key);

        /// <summary>Stores (inserts or updates) the value of the setting with the given key.</summary>
        /// <param name="key">The key of the setting.</param>
        /// <param name="value">The value to store.</param>
        public void SetSetting(string key, string value);

        /// <summary>Returns the value of the setting with the given key which belongs to the given user, or <see langword="null"/> if the user did not set it.</summary>
        /// <param name="userId">The id of the user the setting belongs to.</param>
        /// <param name="key">The key of the setting.</param>
        /// <returns>The stored value, or <see langword="null"/> if the user has no value for the key.</returns>
        public string? GetUserSetting(string userId, string key);

        /// <summary>Stores (inserts or updates) the value of the setting with the given key for the given user.</summary>
        /// <param name="userId">The id of the user the setting belongs to.</param>
        /// <param name="key">The key of the setting.</param>
        /// <param name="value">The value to store.</param>
        public void SetUserSetting(string userId, string key, string value);

        /// <summary>Resets the persistence to its initial empty state.</summary>
        public void Reset();
    }
}
