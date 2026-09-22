using GRYLibrary.Core.APIServer.Services.Database;

namespace OpenIMSBackend.Core.Services
{
    public interface IOpenIMSDatabaseInteractor : IProjectSpecificDatabaseInteractor
    {
        public ISQLProvider GetSQLProvider();
    }
}
