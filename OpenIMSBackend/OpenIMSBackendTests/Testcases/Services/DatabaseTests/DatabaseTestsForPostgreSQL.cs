using GRYLibrary.Core.APIServer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenIMSBackend.Tests.TestUtilities;

namespace OpenIMSBackend.Tests.Testcases.Services.DatabaseTests
{
    [TestClass]
    public class DatabaseTestsForPostgreSQL : DatabaseTestsBase
    {
        private DatabaseTestFrameworkForPostgreSQL? _DatabaseFramework;

        [TestInitialize]
        public void TestInitialize()
        {
            this._DatabaseFramework = TestUtilities.Utilities.GetDatabaseTestFrameworkForPostgreSQL();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this._DatabaseFramework?.Dispose();
            this._DatabaseFramework = null;
        }

        protected override DatabaseTestFrameworkTemplate GetDatabaseTestFramework()
        {
            return this._DatabaseFramework!;
        }

        [TestMethod(DisplayName = nameof(Migration000001Test))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void Migration000001Test()
        {
            this.Migration000001();
        }

        [TestMethod(DisplayName = nameof(AllMigrationsAreWorkingTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void AllMigrationsAreWorkingTest()
        {
            this.AllMigrationsAreWorking();
        }

        [TestMethod(DisplayName = nameof(AllMigrationsAreIdempotentTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void AllMigrationsAreIdempotentTest()
        {
            this.AllMigrationsAreIdempotent();
        }

        [TestMethod(DisplayName = nameof(LoadUserTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void LoadUserTest()
        {
            this.LoadUser();
        }

        [TestMethod(DisplayName = nameof(AccessTokenIsPersistedTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void AccessTokenIsPersistedTest()
        {
            this.AccessTokenIsPersisted();
        }

        [TestMethod(DisplayName = nameof(RoleInheritanceIsPersistedTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void RoleInheritanceIsPersistedTest()
        {
            this.RoleInheritanceIsPersisted();
        }
    }
}
