using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Misc.Migration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenIMSBackend.Core.Misc;
using OpenIMSBackend.Core.Model.BusinessTypes;
using OpenIMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenIMSBackend.Tests.Testcases.Services.DatabaseTests
{
    /// <summary>
    /// The testcases which run against a real database. They exist once here and are executed once per supported
    /// database-type by the derived classes, so that a statement or a migration which only works on one of the two
    /// databases is noticed.
    /// </summary>
    public abstract class DatabaseTestsBase
    {
        protected abstract DatabaseTestFrameworkTemplate GetDatabaseTestFramework();

        protected DatabaseTestFrameworkTemplate GetDatabaseTestFramework(bool runMigrations)
        {
            DatabaseTestFrameworkTemplate result = this.GetDatabaseTestFramework();
            this.PrepareDatabase(result, runMigrations);
            return result;
        }

        /// <summary>
        /// Resets database.
        /// If desired, all available migrations will be done.
        /// </summary>
        private void PrepareDatabase(DatabaseTestFrameworkTemplate databaseTestFramework, bool runMigrations)
        {
            databaseTestFramework.ResetDatabase();
            IGenericDatabaseInteractor databaseInteractor = databaseTestFramework.GenericDatabaseInteractor();
            IOpenIMSDatabaseInteractor openIMSDatabaseInteractor = databaseInteractor.Accept(new GetOpenIMSDatabaseInteractorVisitor());

            List<string> tables1 = databaseInteractor.GetAllTableNames().ToList();
            Assert.IsEmpty(tables1);

            if (runMigrations)
            {
                IList<MigrationInstance> migrations = openIMSDatabaseInteractor.GetAllMigrations();
                Assert.IsNotEmpty(migrations);
                GRYMigrator migrator = new GRYMigrator(new TimeService(), migrations.ToList(), databaseInteractor);
                migrator.InitializeDatabaseAndMigrateIfRequired();
            }
        }

        public abstract void Migration000001Test();
        public void Migration000001()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                using DatabaseTestFrameworkTemplate databaseTestFramework = this.GetDatabaseTestFramework(false);

                //arrange
                IGenericDatabaseInteractor databaseInteractor = databaseTestFramework.GenericDatabaseInteractor();
                IOpenIMSDatabaseInteractor openIMSDatabaseInteractor = databaseInteractor.Accept(new GetOpenIMSDatabaseInteractorVisitor());

                List<string> tables1 = databaseInteractor.GetAllTableNames().ToList();
                Assert.IsEmpty(tables1);

                IList<MigrationInstance> migrations = openIMSDatabaseInteractor.GetAllMigrations();
                GRYMigrator migrator = new GRYMigrator(new TimeService(), migrations.Take(1).ToList(), databaseInteractor);

                //act
                migrator.InitializeDatabaseAndMigrateIfRequired();

                //assert
                Assert.AreEqual(1, migrator.GetExecutedMigrations().Count);
                Assert.AreEqual("Migration000001", migrator.GetExecutedMigrations().First().MigrationName);

                List<string> tables2 = databaseInteractor.GetAllTableNames().ToList();
                foreach (string expectedTable in new[] { "Users", "Roles", "User_Roles", "Role_InheritedRoles", "AccessToken", "RefreshToken", "Settings", "UserSettings" })
                {
                    Assert.IsTrue(tables2.Contains(expectedTable), $"The table '{expectedTable}' was expected to exist after the first migration.");
                }
            }
        }

        public abstract void AllMigrationsAreWorkingTest();
        public void AllMigrationsAreWorking()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                using DatabaseTestFrameworkTemplate databaseTestFramework = this.GetDatabaseTestFramework(false);

                //arrange
                IGenericDatabaseInteractor databaseInteractor = databaseTestFramework.GenericDatabaseInteractor();
                IOpenIMSDatabaseInteractor openIMSDatabaseInteractor = databaseInteractor.Accept(new GetOpenIMSDatabaseInteractorVisitor());

                List<string> tables1 = databaseInteractor.GetAllTableNames().ToList();
                Assert.IsEmpty(tables1);

                IList<MigrationInstance> migrations = openIMSDatabaseInteractor.GetAllMigrations();
                GRYMigrator migrator = new GRYMigrator(new TimeService(), migrations.ToList(), databaseInteractor);

                //act
                migrator.InitializeDatabaseAndMigrateIfRequired();

                //assert
                Assert.AreEqual(migrations.Count, migrator.GetExecutedMigrations().Count);

                List<string> tables2 = databaseInteractor.GetAllTableNames().ToList();
                Assert.IsTrue(1 < tables2.Count);
            }
        }

        public abstract void AllMigrationsAreIdempotentTest();
        /// <summary>
        /// Running the migrations a second time must not do anything, because that is exactly what happens on
        /// every start of a backend whose database is already up to date.
        /// </summary>
        public void AllMigrationsAreIdempotent()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                using DatabaseTestFrameworkTemplate databaseTestFramework = this.GetDatabaseTestFramework(true);

                //arrange
                IGenericDatabaseInteractor databaseInteractor = databaseTestFramework.GenericDatabaseInteractor();
                IOpenIMSDatabaseInteractor openIMSDatabaseInteractor = databaseInteractor.Accept(new GetOpenIMSDatabaseInteractorVisitor());
                List<string> tablesAfterFirstRun = databaseInteractor.GetAllTableNames().ToList();
                GRYMigrator migrator = new GRYMigrator(new TimeService(), openIMSDatabaseInteractor.GetAllMigrations().ToList(), databaseInteractor);
                //GetExecutedMigrations reads the migration-maintenance-table, so it answers which migrations were
                //ever executed against this database and not which ones this migrator-instance executed.
                List<MigrationExecutionInformation> executedMigrationsBefore = migrator.GetExecutedMigrations().ToList();

                //act
                migrator.InitializeDatabaseAndMigrateIfRequired();

                //assert
                List<MigrationExecutionInformation> executedMigrationsAfter = migrator.GetExecutedMigrations().ToList();
                CollectionAssert.AreEquivalent(
                    executedMigrationsBefore.Select(migration => migration.MigrationName).ToList(),
                    executedMigrationsAfter.Select(migration => migration.MigrationName).ToList(),
                    "A migration was executed a second time.");
                CollectionAssert.AreEquivalent(tablesAfterFirstRun, databaseInteractor.GetAllTableNames().ToList());
            }
        }

        public abstract void LoadUserTest();
        /// <summary>
        /// Writes a user into a real database and reads it back. This is what verifies that the statements
        /// "AddUser", "GetUserById" and "GetUserByName" really work against that database-type.
        /// </summary>
        public void LoadUser()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                using DatabaseTestFrameworkTemplate databaseTestFramework = this.GetDatabaseTestFramework(true);

                //arrange
                using IGenericDatabaseInteractor databaseInteractor = databaseTestFramework.GenericDatabaseInteractor();
                IOpenIMSDatabaseInteractor openIMSDatabaseInteractor = databaseInteractor.Accept(new GetOpenIMSDatabaseInteractorVisitor());
                ITimeService timeService = new TimeService();
                IServerLog log = ServerLog.GetTransientLog();
                using DatabasePersistence databasePersistence = new DatabasePersistence(openIMSDatabaseInteractor, timeService, log);
                User expectedUser = User.Create("testuser", "hash-of-the-password", timeService);
                databasePersistence.AddUser(expectedUser);

                //act
                User actualUserById = databasePersistence.GetUserById(expectedUser.Id);
                User actualUserByName = databasePersistence.GetUserByName(expectedUser.Name);

                //assert
                Assert.AreEqual(expectedUser.Id, actualUserById.Id);
                Assert.AreEqual(expectedUser.Name, actualUserById.Name);
                Assert.AreEqual(expectedUser.PasswordHash, actualUserById.PasswordHash);
                Assert.AreEqual(expectedUser.Id, actualUserByName.Id);
                Assert.AreEqual(expectedUser.Name, actualUserByName.Name);
            }
        }

        public abstract void AccessTokenIsPersistedTest();
        /// <summary>
        /// An access-token is what the whole authentication depends on, so it is verified against a real database
        /// that it can be written, read back and removed again.
        /// </summary>
        public void AccessTokenIsPersisted()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                using DatabaseTestFrameworkTemplate databaseTestFramework = this.GetDatabaseTestFramework(true);

                //arrange
                using IGenericDatabaseInteractor databaseInteractor = databaseTestFramework.GenericDatabaseInteractor();
                IOpenIMSDatabaseInteractor openIMSDatabaseInteractor = databaseInteractor.Accept(new GetOpenIMSDatabaseInteractorVisitor());
                ITimeService timeService = new TimeService();
                using DatabasePersistence databasePersistence = new DatabasePersistence(openIMSDatabaseInteractor, timeService, ServerLog.GetTransientLog());
                User user = User.Create("tokenowner", "hash-of-the-password", timeService);
                databasePersistence.AddUser(user);
                GRYLibrary.Core.APIServer.CommonAuthenticationTypes.AccessToken expectedAccessToken = new GRYLibrary.Core.APIServer.CommonAuthenticationTypes.AccessToken()
                {
                    Value = Guid.NewGuid().ToString(),
                    ExpiredMoment = timeService.GetCurrentLocalTimeAsDateTimeOffset().AddDays(1),
                    OwnerUserId = user.Id
                };

                //act
                databasePersistence.AddAccessToken(expectedAccessToken);
                GRYLibrary.Core.APIServer.CommonAuthenticationTypes.AccessToken actualAccessToken = databasePersistence.GetAccessToken(expectedAccessToken.Value);
                User ownerOfTheToken = databasePersistence.GetUserByAccessToken(expectedAccessToken.Value);
                databasePersistence.RemoveAccessToken(expectedAccessToken.Value);

                //assert
                Assert.AreEqual(expectedAccessToken.Value, actualAccessToken.Value);
                Assert.AreEqual(user.Id, actualAccessToken.OwnerUserId);
                Assert.AreEqual(user.Id, ownerOfTheToken.Id);
                Assert.ThrowsExactly<KeyNotFoundException>(() => databasePersistence.GetAccessToken(expectedAccessToken.Value));
            }
        }

        public abstract void RoleInheritanceIsPersistedTest();
        /// <summary>
        /// The inheritance of a role decides which endpoints a user reaches, and it is the only part of the
        /// role-handling which is written with a generated statement, so it is verified against a real database.
        /// </summary>
        public void RoleInheritanceIsPersisted()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                using DatabaseTestFrameworkTemplate databaseTestFramework = this.GetDatabaseTestFramework(true);

                //arrange
                using IGenericDatabaseInteractor databaseInteractor = databaseTestFramework.GenericDatabaseInteractor();
                IOpenIMSDatabaseInteractor openIMSDatabaseInteractor = databaseInteractor.Accept(new GetOpenIMSDatabaseInteractorVisitor());
                using DatabasePersistence databasePersistence = new DatabasePersistence(openIMSDatabaseInteractor, new TimeService(), ServerLog.GetTransientLog());
                GRYLibrary.Core.APIServer.CommonDBTypes.Role usersRole = new GRYLibrary.Core.APIServer.CommonDBTypes.Role() { Id = Guid.NewGuid().ToString(), Name = "Users", DirectlyInheritedRoles = new HashSet<GRYLibrary.Core.APIServer.CommonDBTypes.Role>() };
                GRYLibrary.Core.APIServer.CommonDBTypes.Role adminsRole = new GRYLibrary.Core.APIServer.CommonDBTypes.Role() { Id = Guid.NewGuid().ToString(), Name = "Administrators", DirectlyInheritedRoles = new HashSet<GRYLibrary.Core.APIServer.CommonDBTypes.Role>() };
                databasePersistence.AddRole(usersRole);
                databasePersistence.AddRole(adminsRole);
                adminsRole.DirectlyInheritedRoles.Add(usersRole);

                //act
                databasePersistence.UpdateRole(adminsRole);
                GRYLibrary.Core.APIServer.CommonDBTypes.Role actualAdminsRole = databasePersistence.GetAllRoles().Single(role => role.Id == adminsRole.Id);

                //assert
                Assert.AreEqual(1, actualAdminsRole.DirectlyInheritedRoles.Count);
                Assert.AreEqual(usersRole.Id, actualAdminsRole.DirectlyInheritedRoles.Single().Id);
            }
        }
    }
}
