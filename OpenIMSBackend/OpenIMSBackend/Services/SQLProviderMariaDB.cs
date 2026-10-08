using OpenIMSBackend.Core.Misc;

namespace OpenIMSBackend.Core.Services
{
    public class SQLProviderMariaDB : SQLProvider
    {
        /// <summary>Initializes a new <see cref="SQLProviderMariaDB"/> pointing to the MariaDB statement resources.</summary>
        public SQLProviderMariaDB() : base("MariaDB") { }
    }
}
