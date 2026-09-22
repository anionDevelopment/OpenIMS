using GRYLibrary.Core.APIServer.Services.Database;
using OpenIMSBackend.Core.Services;
using System;

namespace OpenIMSBackend.Core.Misc
{
    public class GetOpenIMSDatabaseInteractorVisitor : IGenericDatabaseInteractorVisitor<IOpenIMSDatabaseInteractor>
    {

        /// <inheritdoc />
        public IOpenIMSDatabaseInteractor Handle(MariaDBDatabaseInteractor mariaDBDatabaseInteractor)
        {
            return new DatabaseInteractorMariaDB(mariaDBDatabaseInteractor);
        }

        /// <inheritdoc />
        public IOpenIMSDatabaseInteractor Handle(OracleDatabaseInteractor oracleDatabaseInteractor)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public IOpenIMSDatabaseInteractor Handle(SQLServerDatabaseInteractor sQLServerDatabaseInteractor)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public IOpenIMSDatabaseInteractor Handle(PostgreSQLDatabaseInteractor postgreSQLDatabaseInteractor)
        {
            return new DatabaseInteractorPostgreSQL(postgreSQLDatabaseInteractor);
        }
    }
}
