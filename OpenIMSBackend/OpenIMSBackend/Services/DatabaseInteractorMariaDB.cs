using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.Misc.Migration;
using System.Collections.Generic;
using System.Reflection;

namespace OpenIMSBackend.Core.Services
{
    public class DatabaseInteractorMariaDB : IOpenIMSDatabaseInteractor
    {
        private readonly MariaDBDatabaseInteractor _DatabaseInteractor;
        private readonly IList<MigrationInstance> _Migrations = GRYMigrator.LoadMigrationsFromResources(Assembly.GetExecutingAssembly(), "OpenIMSBackend.Core.Resources.Database.MariaDB.Migrations.");

        /// <summary>Initializes a new instance of <see cref="DatabaseInteractorMariaDB"/>.</summary>
        /// <param name="interactor">The generic database interactor to wrap.</param>
        public DatabaseInteractorMariaDB(IGenericDatabaseInteractor interactor)
        {
            this._DatabaseInteractor = (MariaDBDatabaseInteractor)interactor;
        }

        /// <summary>Returns all registered database migrations for MariaDB.</summary>
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

        /// <summary>Returns the SQL provider for MariaDB.</summary>
        /// <returns>An <see cref="ISQLProvider"/> backed by MariaDB statements.</returns>
        public ISQLProvider GetSQLProvider()
        {
            return new SQLProviderMariaDB();
        }

        /// <summary>Enables or disables logging of connection-attempt errors.</summary>
        /// <param name="enabled">True to enable logging; false to disable.</param>
        public void SetLogConnectionAttemptErrors(bool enabled)
        {
            this._DatabaseInteractor.SetLogConnectionAttemptErrors(enabled);
        }
    }
}
