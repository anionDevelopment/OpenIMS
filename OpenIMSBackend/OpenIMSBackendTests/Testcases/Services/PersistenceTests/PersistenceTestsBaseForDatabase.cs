using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Misc.Migration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenIMSBackend.Core.Misc;
using OpenIMSBackend.Core.Services;
using OpenIMSBackend.Tests.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenIMSBackend.Tests.Testcases.Services.PersistenceTests
{
    /// <summary>
    /// Runs the persistence-testcases against a real, freshly-migrated database. The concrete database-type is
    /// decided by the derived class.
    /// </summary>
    public abstract class PersistenceTestsBaseForDatabase : PersistenceTestsBase
    {
        protected abstract DatabaseTestFrameworkTemplate GetDatabaseTestFramework();

        public override void Dispose()
        {
            GRYLibrary.Core.Misc.Utilities.NoOperation();
        }

        internal override PersistenceDisposable GetPersistence(ITimeService timeService)
        {
            DatabaseTestFrameworkTemplate databaseTestFramework = this.GetDatabaseTestFramework();
            IServerLog logger = ServerLog.GetTransientLog();
            IPersistence result = new DatabasePersistence(databaseTestFramework.GenericDatabaseInteractor().Accept(new GetOpenIMSDatabaseInteractorVisitor()), timeService, logger);

            databaseTestFramework.ResetDatabase();
            IGenericDatabaseInteractor databaseInteractor = databaseTestFramework.GenericDatabaseInteractor();
            IOpenIMSDatabaseInteractor openIMSDatabaseInteractor = databaseInteractor.Accept(new GetOpenIMSDatabaseInteractorVisitor());

            List<string> tables = databaseInteractor.GetAllTableNames().ToList();
            Assert.IsEmpty(tables);

            IList<MigrationInstance> migrations = openIMSDatabaseInteractor.GetAllMigrations();
            GRYMigrator migrator = new GRYMigrator(new TimeService(), migrations.ToList(), databaseInteractor);
            migrator.InitializeDatabaseAndMigrateIfRequired();

            return new PersistenceDisposable(result, new HashSet<IDisposable>() { databaseTestFramework, databaseInteractor });
        }
    }
}
