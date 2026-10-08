using GRYLibrary.Core.APIServer.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenIMSBackend.Core.Services;
using OpenIMSBackend.Tests.TestUtilities;
using System;
using System.Collections.Generic;

namespace OpenIMSBackend.Tests.Testcases.Services.PersistenceTests
{
    /// <summary>
    /// Runs the persistence-testcases against the transient persistence. These are unit-tests, because the
    /// transient persistence needs no database and therefore no test-service.
    /// </summary>
    [TestClass]
    public class PersistenceTestsForTransient : PersistenceTestsBase
    {
        internal override PersistenceDisposable GetPersistence(ITimeService timeService)
        {
            (TransientPersistence, ISet<IDisposable>) result = TestUtilities.Utilities.GetTransientPersistence(timeService);
            return new PersistenceDisposable(result.Item1, result.Item2);
        }

        public override void Dispose()
        {
            GRYLibrary.Core.Misc.Utilities.NoOperation();
        }

        [TestMethod(DisplayName = nameof(AddUserTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void AddUserTest()
        {
            this.AddUser();
        }

        [TestMethod(DisplayName = nameof(GetAmountOfUsersTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void GetAmountOfUsersTest()
        {
            this.GetAmountOfUsers();
        }

        [TestMethod(DisplayName = nameof(GetUnknownUserThrowsTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void GetUnknownUserThrowsTest()
        {
            this.GetUnknownUserThrows();
        }

        [TestMethod(DisplayName = nameof(AddRoleToUserTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void AddRoleToUserTest()
        {
            this.AddRoleToUser();
        }

        [TestMethod(DisplayName = nameof(RemoveRoleFromUserTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void RemoveRoleFromUserTest()
        {
            this.RemoveRoleFromUser();
        }

        [TestMethod(DisplayName = nameof(RoleExistsTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void RoleExistsTest()
        {
            this.RoleExists();
        }

        [TestMethod(DisplayName = nameof(AccessTokenRoundTripTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void AccessTokenRoundTripTest()
        {
            this.AccessTokenRoundTrip();
        }

        [TestMethod(DisplayName = nameof(RemoveAccessTokenTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void RemoveAccessTokenTest()
        {
            this.RemoveAccessToken();
        }

        [TestMethod(DisplayName = nameof(SettingRoundTripTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void SettingRoundTripTest()
        {
            this.SettingRoundTrip();
        }

        [TestMethod(DisplayName = nameof(UserSettingRoundTripTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void UserSettingRoundTripTest()
        {
            this.UserSettingRoundTrip();
        }

        [TestMethod(DisplayName = nameof(InventoryObjectRoundTripTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void InventoryObjectRoundTripTest()
        {
            this.InventoryObjectRoundTrip();
        }

        [TestMethod(DisplayName = nameof(InventoryObjectReferencesTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void InventoryObjectReferencesTest()
        {
            this.InventoryObjectReferences();
        }

        [TestMethod(DisplayName = nameof(RolesOfAuthorizableItemTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void RolesOfAuthorizableItemTest()
        {
            this.RolesOfAuthorizableItem();
        }

        [TestMethod(DisplayName = nameof(SearchInventoryObjectsTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void SearchInventoryObjectsTest()
        {
            this.SearchInventoryObjects();
        }

        [TestMethod(DisplayName = nameof(DeleteRoleTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void DeleteRoleTest()
        {
            this.DeleteRole();
        }
    }
}
