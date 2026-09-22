using OpenIMSBackend.Core.Misc;

namespace OpenIMSBackend.Core.Services
{
    public class SQLProviderPostgreSQL : SQLProvider
    {
        /// <summary>Initializes a new <see cref="SQLProviderPostgreSQL"/> pointing to the PostgreSQL statement resources.</summary>
        public SQLProviderPostgreSQL() : base("PostgreSQL") { }
    }
}
