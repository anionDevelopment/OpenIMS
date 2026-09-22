using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Misc.Migration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenIMSBackend.Core.Misc;
using OpenIMSBackend.Core.Services;
using OpenIMSBackend.Tests.TestUtilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenIMSBackend.Tests.Testcases.Services
{
    /// <summary>
    /// Runs all migrations against a real, freshly-reset MariaDB test-database and generates the MariaDB entity-relationship diagram of the resulting
    /// schema (read back from <c>information_schema</c>, not from the migration-SQL) as a <c>.plantuml</c> file for the reference-documentation.
    /// </summary>
    [TestClass]
    public class DatabaseStructureTestsForMariaDB
    {
        private DatabaseTestFrameworkForMariaDB? _DatabaseTestFramework;

        [TestInitialize]
        public void TestInitialize()
        {
            this._DatabaseTestFramework = TestUtilities.Utilities.GetDatabaseTestFrameworkForMariaDB();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this._DatabaseTestFramework?.Dispose();
            this._DatabaseTestFramework = null;
        }

        [TestMethod(DisplayName = nameof(GenerateDatabaseStructureDiagram))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void GenerateDatabaseStructureDiagram()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                DatabaseTestFrameworkForMariaDB databaseTestFramework = this._DatabaseTestFramework!;

                //arrange
                databaseTestFramework.ResetDatabase();
                IGenericDatabaseInteractor databaseInteractor = databaseTestFramework.GenericDatabaseInteractor();
                IOpenIMSDatabaseInteractor codeunitSpecificDatabaseInteractor = databaseInteractor.Accept(new GetOpenIMSDatabaseInteractorVisitor());
                IList<MigrationInstance> migrations = codeunitSpecificDatabaseInteractor.GetAllMigrations();
                GRYMigrator migrator = new GRYMigrator(new TimeService(), migrations.ToList(), databaseInteractor);
                migrator.InitializeDatabaseAndMigrateIfRequired();

                string targetFile = Path.Combine(TestUtilities.Constants.GeneralConstants.CodeUnitFolder, "Other", "Reference", "ReferenceContent", "Articles", "Diagrams", "DatabaseStructure.MariaDB.plantuml");

                //act
                string plantUml = DatabaseStructureDiagramGenerator.Generate(codeunitSpecificDatabaseInteractor, ServerLog.GetTransientLog().Logger, "OpenIMSBackend - Database structure (MariaDB)", targetFile);

                //assert
                Assert.IsTrue(plantUml.Contains("@startuml"));
                Assert.IsTrue(plantUml.Contains("@enduml"));
                foreach (string expectedTable in new[] { "Users", "Roles", "User_Roles", "Role_InheritedRoles", "AccessToken", "RefreshToken", "Settings", "UserSettings" })
                {
                    Assert.IsTrue(plantUml.Contains($"entity \"{expectedTable}\""), $"The table '{expectedTable}' was expected to be part of the diagram.");
                }
                Assert.IsFalse(plantUml.Contains("GRYMigrationInformation"));
                Assert.IsTrue(plantUml.Contains("}o--||"));
            }
        }
    }
}
