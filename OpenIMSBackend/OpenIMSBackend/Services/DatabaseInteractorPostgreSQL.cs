using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc.Migration;
using System.Collections.Generic;
using System.Reflection;

namespace OpenIMSBackend.Core.Services
{
    public class DatabaseInteractorPostgreSQL : IOpenIMSDatabaseInteractor
    {
        public IGRYLog Log
        {
            get
            {
                return this._DatabaseInteractor.Log;
            }
        }
        private readonly PostgreSQLDatabaseInteractor _DatabaseInteractor;
        private readonly IList<MigrationInstance> _Migrations = GRYMigrator.LoadMigrationsFromResources(Assembly.GetExecutingAssembly(), "OpenIMSBackend.Core.Resources.Database.PostgreSQL.Migrations.");

        /// <summary>Initializes a new instance of <see cref="DatabaseInteractorPostgreSQL"/>.</summary>
        /// <param name="interactor">The generic database interactor to wrap.</param>
        public DatabaseInteractorPostgreSQL(IGenericDatabaseInteractor interactor)
        {
            this._DatabaseInteractor = (PostgreSQLDatabaseInteractor)interactor;
        }

        /// <summary>Returns all registered database migrations for PostgreSQL.</summary>
        /// <returns>A list of <see cref="MigrationInstance"/> objects.</returns>
        public IList<MigrationInstance> GetAllMigrations()
        {
            return this._Migrations;
        }

        /// <summary>Returns the underlying generic database interactor.</summary>
        /// <returns>The <see cref="IGenericDatabaseInteractor"/> instance.</returns>
        public IGenericDatabaseInteractor GetGenericDatabaseInteractor()
        {
            return this._DatabaseInteractor;
        }

        /// <summary>Returns the SQL provider for PostgreSQL.</summary>
        /// <returns>An <see cref="ISQLProvider"/> backed by PostgreSQL statements.</returns>
        public ISQLProvider GetSQLProvider()
        {
            return new SQLProviderPostgreSQL();
        }

        /// <summary>Enables or disables logging of connection-attempt errors.</summary>
        /// <param name="enabled">True to enable logging; false to disable.</param>
        public void SetLogConnectionAttemptErrors(bool enabled)
        {
            this._DatabaseInteractor.SetLogConnectionAttemptErrors(enabled);
        }
    }
}
