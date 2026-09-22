using GRYLibrary.Core.APIServer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenIMSBackend.Tests.TestUtilities;

namespace OpenIMSBackend.Tests.Testcases.Services.PersistenceTests
{
    [TestClass]
    public class PersistenceTestsForMariaDB : PersistenceTestsBaseForDatabase
    {
        private DatabaseTestFrameworkForMariaDB? _DatabaseFramework;

        [TestInitialize]
        public void TestInitialize()
        {
            this._DatabaseFramework = TestUtilities.Utilities.GetDatabaseTestFrameworkForMariaDB();
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

        [TestMethod(DisplayName = nameof(AddUserTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void AddUserTest()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                this.AddUser();
            }
        }

        [TestMethod(DisplayName = nameof(GetAmountOfUsersTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void GetAmountOfUsersTest()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                this.GetAmountOfUsers();
            }
        }

        [TestMethod(DisplayName = nameof(GetUnknownUserThrowsTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void GetUnknownUserThrowsTest()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                this.GetUnknownUserThrows();
            }
        }

        [TestMethod(DisplayName = nameof(AddRoleToUserTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void AddRoleToUserTest()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                this.AddRoleToUser();
            }
        }

        [TestMethod(DisplayName = nameof(RemoveRoleFromUserTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void RemoveRoleFromUserTest()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                this.RemoveRoleFromUser();
            }
        }

        [TestMethod(DisplayName = nameof(RoleExistsTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void RoleExistsTest()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                this.RoleExists();
            }
        }

        [TestMethod(DisplayName = nameof(AccessTokenRoundTripTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void AccessTokenRoundTripTest()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                this.AccessTokenRoundTrip();
            }
        }

        [TestMethod(DisplayName = nameof(RemoveAccessTokenTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void RemoveAccessTokenTest()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                this.RemoveAccessToken();
            }
        }

        [TestMethod(DisplayName = nameof(SettingRoundTripTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void SettingRoundTripTest()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                this.SettingRoundTrip();
            }
        }

        [TestMethod(DisplayName = nameof(UserSettingRoundTripTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void UserSettingRoundTripTest()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                this.UserSettingRoundTrip();
            }
        }
    }
}
