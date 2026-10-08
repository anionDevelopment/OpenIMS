using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenIMSBackend.Core.Model.PersistenceTypes;
using OpenIMSBackend.Core.Services;
using OpenIMSBackend.Tests.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenIMSBackend.Tests.Testcases.Services.PersistenceTests
{
    /// <summary>
    /// The testcases which every implementation of <see cref="IPersistence"/> has to fulfill. They exist once
    /// here and are executed once per implementation by the derived classes, so that the transient persistence
    /// and the two database-persistences really behave the same way.
    /// </summary>
    public abstract class PersistenceTestsBase : IDisposable
    {
        internal abstract PersistenceDisposable GetPersistence(ITimeService timeService);
        public abstract void Dispose();

        private static Core.Model.BusinessTypes.User CreateUser(string name, ITimeService timeService)
        {
            return Core.Model.BusinessTypes.User.Create(name, "hash-of-" + name, timeService);
        }

        public abstract void AddUserTest();
        public void AddUser()
        {
            ITimeService timeService = new TimeService();
            using PersistenceDisposable persistenceDisposable = this.GetPersistence(timeService);
            IPersistence persistence = persistenceDisposable.Persistence;

            //arrange
            Core.Model.BusinessTypes.User expectedUser = CreateUser("alice", timeService);

            //act
            persistence.AddUser(expectedUser);

            //assert
            Assert.IsTrue(persistence.UserWithIdExists(expectedUser.Id));
            Assert.IsTrue(persistence.UserWithNameExists(expectedUser.Name));
            Assert.AreEqual(expectedUser.Name, persistence.GetUserById(expectedUser.Id).Name);
            Assert.AreEqual(expectedUser.Id, persistence.GetUserByName(expectedUser.Name).Id);
        }

        public abstract void GetAmountOfUsersTest();
        public void GetAmountOfUsers()
        {
            ITimeService timeService = new TimeService();
            using PersistenceDisposable persistenceDisposable = this.GetPersistence(timeService);
            IPersistence persistence = persistenceDisposable.Persistence;

            //arrange
            uint amountBefore = persistence.GetAmountOfUsers();

            //act
            persistence.AddUser(CreateUser("bob", timeService));
            persistence.AddUser(CreateUser("carol", timeService));

            //assert
            Assert.AreEqual(amountBefore + 2, persistence.GetAmountOfUsers());
        }

        public abstract void GetUnknownUserThrowsTest();
        /// <summary>
        /// A caller has to be able to distinguish "this user does not exist" from "the persistence answered
        /// nothing", so an unknown user must not be answered with null.
        /// </summary>
        public void GetUnknownUserThrows()
        {
            ITimeService timeService = new TimeService();
            using PersistenceDisposable persistenceDisposable = this.GetPersistence(timeService);
            IPersistence persistence = persistenceDisposable.Persistence;

            //act + assert
            //Which exception is thrown is not part of the contract and differs between the implementations
            //(the database-implementation throws a KeyNotFoundException, the transient one lets the lookup of
            //its underlying collection fail). What has to hold for every implementation is that an unknown user
            //is not answered with null, because a caller could not distinguish that from "no answer".
            Assert.Throws<Exception>(() => persistence.GetUserById("this-id-does-not-exist"));
            Assert.Throws<Exception>(() => persistence.GetUserByName("this-name-does-not-exist"));
        }

        public abstract void AddRoleToUserTest();
        public void AddRoleToUser()
        {
            ITimeService timeService = new TimeService();
            using PersistenceDisposable persistenceDisposable = this.GetPersistence(timeService);
            IPersistence persistence = persistenceDisposable.Persistence;

            //arrange
            Core.Model.BusinessTypes.User user = CreateUser("dave", timeService);
            persistence.AddUser(user);
            Role role = new Role() { Id = Guid.NewGuid().ToString(), Name = "Users", DirectlyInheritedRoles = new HashSet<Role>() };
            persistence.AddRole(role);

            //act
            persistence.AddRoleToUser(user.Id, role.Id);

            //assert
            Assert.IsTrue(persistence.UserHasRole(user.Id, role.Id));
            Assert.IsTrue(persistence.GetUserById(user.Id).Roles.Any(r => r.Id == role.Id));
        }

        public abstract void RemoveRoleFromUserTest();
        public void RemoveRoleFromUser()
        {
            ITimeService timeService = new TimeService();
            using PersistenceDisposable persistenceDisposable = this.GetPersistence(timeService);
            IPersistence persistence = persistenceDisposable.Persistence;

            //arrange
            Core.Model.BusinessTypes.User user = CreateUser("erin", timeService);
            persistence.AddUser(user);
            Role role = new Role() { Id = Guid.NewGuid().ToString(), Name = "Users", DirectlyInheritedRoles = new HashSet<Role>() };
            persistence.AddRole(role);
            persistence.AddRoleToUser(user.Id, role.Id);

            //act
            persistence.RemoveRoleFromUser(user.Id, role.Id);

            //assert
            Assert.IsFalse(persistence.UserHasRole(user.Id, role.Id));
        }

        public abstract void RoleExistsTest();
        public void RoleExists()
        {
            ITimeService timeService = new TimeService();
            using PersistenceDisposable persistenceDisposable = this.GetPersistence(timeService);
            IPersistence persistence = persistenceDisposable.Persistence;

            //arrange
            Role role = new Role() { Id = Guid.NewGuid().ToString(), Name = "Administrators", DirectlyInheritedRoles = new HashSet<Role>() };

            //act
            bool existsBefore = persistence.RoleExists(role.Name);
            persistence.AddRole(role);
            bool existsAfter = persistence.RoleExists(role.Name);

            //assert
            Assert.IsFalse(existsBefore);
            Assert.IsTrue(existsAfter);
            Assert.AreEqual(role.Id, persistence.GetRoleByName(role.Name).Id);
            Assert.AreEqual(role.Name, persistence.GetRoleById(role.Id).Name);
        }

        public abstract void AccessTokenRoundTripTest();
        public void AccessTokenRoundTrip()
        {
            ITimeService timeService = new TimeService();
            using PersistenceDisposable persistenceDisposable = this.GetPersistence(timeService);
            IPersistence persistence = persistenceDisposable.Persistence;

            //arrange
            Core.Model.BusinessTypes.User user = CreateUser("frank", timeService);
            persistence.AddUser(user);
            AccessToken accessToken = new AccessToken()
            {
                Value = Guid.NewGuid().ToString(),
                ExpiredMoment = timeService.GetCurrentLocalTimeAsDateTimeOffset().AddDays(1),
                OwnerUserId = user.Id
            };

            //act
            //A token is registered the same way the authentication-service registers it: at the user it belongs
            //to and in the persistence. The transient implementation resolves a token to its user through the
            //token-collection of the user, so registering it only in the persistence would not be enough there.
            user.AccessToken.Add(accessToken);
            persistence.AddAccessToken(accessToken);

            //assert
            Assert.AreEqual(user.Id, persistence.GetAccessToken(accessToken.Value).OwnerUserId);
            Assert.AreEqual(user.Id, persistence.GetUserByAccessToken(accessToken.Value).Id);
            Assert.IsTrue(persistence.GetAllAccessTokenOfUser(user.Id).Any(token => token.Value == accessToken.Value));
        }

        public abstract void RemoveAccessTokenTest();
        /// <summary>This is what a logout does, so a removed token really must not be resolvable anymore.</summary>
        public void RemoveAccessToken()
        {
            ITimeService timeService = new TimeService();
            using PersistenceDisposable persistenceDisposable = this.GetPersistence(timeService);
            IPersistence persistence = persistenceDisposable.Persistence;

            //arrange
            Core.Model.BusinessTypes.User user = CreateUser("grace", timeService);
            persistence.AddUser(user);
            AccessToken accessToken = new AccessToken()
            {
                Value = Guid.NewGuid().ToString(),
                ExpiredMoment = timeService.GetCurrentLocalTimeAsDateTimeOffset().AddDays(1),
                OwnerUserId = user.Id
            };
            persistence.AddAccessToken(accessToken);

            //act
            persistence.RemoveAccessToken(accessToken.Value);

            //assert
            Assert.ThrowsExactly<KeyNotFoundException>(() => persistence.GetAccessToken(accessToken.Value));
        }

        public abstract void SettingRoundTripTest();
        public void SettingRoundTrip()
        {
            ITimeService timeService = new TimeService();
            using PersistenceDisposable persistenceDisposable = this.GetPersistence(timeService);
            IPersistence persistence = persistenceDisposable.Persistence;

            //act + assert
            Assert.IsNull(persistence.GetSetting("SomeKey"), "A setting which was never set has to be answered with null.");
            persistence.SetSetting("SomeKey", "SomeValue");
            Assert.AreEqual("SomeValue", persistence.GetSetting("SomeKey"));
            //setting it again has to update the value instead of failing with a duplicate-key-error.
            persistence.SetSetting("SomeKey", "AnotherValue");
            Assert.AreEqual("AnotherValue", persistence.GetSetting("SomeKey"));
        }

        public abstract void UserSettingRoundTripTest();
        public void UserSettingRoundTrip()
        {
            ITimeService timeService = new TimeService();
            using PersistenceDisposable persistenceDisposable = this.GetPersistence(timeService);
            IPersistence persistence = persistenceDisposable.Persistence;

            //arrange
            Core.Model.BusinessTypes.User userA = CreateUser("heidi", timeService);
            Core.Model.BusinessTypes.User userB = CreateUser("ivan", timeService);
            persistence.AddUser(userA);
            persistence.AddUser(userB);

            //act
            persistence.SetUserSetting(userA.Id, "Theme", "dark");

            //assert
            Assert.AreEqual("dark", persistence.GetUserSetting(userA.Id, "Theme"));
            Assert.IsNull(persistence.GetUserSetting(userB.Id, "Theme"), "The setting of one user must not be visible for another user.");
            persistence.SetUserSetting(userA.Id, "Theme", "light");
            Assert.AreEqual("light", persistence.GetUserSetting(userA.Id, "Theme"));
        }

        public abstract void InventoryObjectRoundTripTest();
        public void InventoryObjectRoundTrip()
        {
            ITimeService timeService = new TimeService();
            using PersistenceDisposable persistenceDisposable = this.GetPersistence(timeService);
            IPersistence persistence = persistenceDisposable.Persistence;

            //arrange
            InventoryObjectRecord location = new InventoryObjectRecord("location-1", InventoryObjectType.Location, "Location", "A description", null, null);
            InventoryObjectRecord area = new InventoryObjectRecord("area-1", InventoryObjectType.Area, "Area", string.Empty, location.Id, null);
            InventoryObjectRecord item = new InventoryObjectRecord("item-1", InventoryObjectType.Item, "Item", string.Empty, area.Id, area.Id);

            //act
            persistence.AddInventoryObject(location);
            persistence.AddInventoryObject(area);
            persistence.AddInventoryObject(item);

            //assert
            Assert.IsTrue(persistence.InventoryObjectExists(item.Id));
            Assert.IsFalse(persistence.InventoryObjectExists("does-not-exist"));
            InventoryObjectRecord loadedLocation = persistence.GetInventoryObject(location.Id);
            Assert.AreEqual(InventoryObjectType.Location, loadedLocation.Type);
            Assert.AreEqual("Location", loadedLocation.Name);
            Assert.AreEqual("A description", loadedLocation.Description);
            Assert.IsNull(loadedLocation.ParentId);
            Assert.IsNull(loadedLocation.RealPlaceId);
            InventoryObjectRecord loadedItem = persistence.GetInventoryObject(item.Id);
            Assert.AreEqual(InventoryObjectType.Item, loadedItem.Type);
            Assert.AreEqual(area.Id, loadedItem.ParentId);
            Assert.AreEqual(area.Id, loadedItem.RealPlaceId);
            Assert.HasCount(3, persistence.GetAllInventoryObjects());

            //act
            loadedItem.Name = "Renamed item";
            loadedItem.Description = "Changed description";
            persistence.UpdateInventoryObject(loadedItem);

            //assert
            InventoryObjectRecord updatedItem = persistence.GetInventoryObject(item.Id);
            Assert.AreEqual("Renamed item", updatedItem.Name);
            Assert.AreEqual("Changed description", updatedItem.Description);
        }

        public abstract void InventoryObjectReferencesTest();
        public void InventoryObjectReferences()
        {
            ITimeService timeService = new TimeService();
            using PersistenceDisposable persistenceDisposable = this.GetPersistence(timeService);
            IPersistence persistence = persistenceDisposable.Persistence;

            //arrange
            persistence.AddInventoryObject(new InventoryObjectRecord("location-1", InventoryObjectType.Location, "Location", string.Empty, null, null));
            persistence.AddInventoryObject(new InventoryObjectRecord("area-1", InventoryObjectType.Area, "Area", string.Empty, "location-1", null));
            persistence.AddInventoryObject(new InventoryObjectRecord("container-1", InventoryObjectType.Container, "Container", string.Empty, "area-1", null));
            persistence.AddInventoryObject(new InventoryObjectRecord("item-1", InventoryObjectType.Item, "Item", string.Empty, "area-1", "container-1"));

            //act + assert
            Assert.IsTrue(persistence.InventoryObjectHasChildren("area-1"));
            Assert.IsFalse(persistence.InventoryObjectHasChildren("container-1"));
            Assert.IsTrue(persistence.InventoryObjectIsUsedAsRealPlace("container-1"));
            Assert.IsFalse(persistence.InventoryObjectIsUsedAsRealPlace("area-1"));

            persistence.RemoveInventoryObject("item-1");
            Assert.IsFalse(persistence.InventoryObjectExists("item-1"));
            Assert.IsFalse(persistence.InventoryObjectIsUsedAsRealPlace("container-1"));
        }

        public abstract void RolesOfAuthorizableItemTest();
        public void RolesOfAuthorizableItem()
        {
            ITimeService timeService = new TimeService();
            using PersistenceDisposable persistenceDisposable = this.GetPersistence(timeService);
            IPersistence persistence = persistenceDisposable.Persistence;

            //arrange
            Role roleA = new Role() { Id = Guid.NewGuid().ToString(), Name = "GroupA", DirectlyInheritedRoles = new HashSet<Role>() };
            Role roleB = new Role() { Id = Guid.NewGuid().ToString(), Name = "GroupB", DirectlyInheritedRoles = new HashSet<Role>() };
            persistence.AddRole(roleA);
            persistence.AddRole(roleB);
            persistence.AddInventoryObject(new InventoryObjectRecord("location-1", InventoryObjectType.Location, "Location", string.Empty, null, null));
            persistence.AddInventoryObject(new InventoryObjectRecord("location-2", InventoryObjectType.Location, "Location 2", string.Empty, null, null));

            //act
            persistence.AddRoleToAuthorizableItem(roleA.Id, "location-1");
            persistence.AddRoleToAuthorizableItem(roleB.Id, "location-1");
            persistence.AddRoleToAuthorizableItem(roleA.Id, "location-2");

            //assert
            CollectionAssert.AreEquivalent(new List<string>() { roleA.Id, roleB.Id }, persistence.GetRoleIdsOfAuthorizableItem("location-1").ToList());
            persistence.RemoveRoleFromAuthorizableItem(roleB.Id, "location-1");
            CollectionAssert.AreEquivalent(new List<string>() { roleA.Id }, persistence.GetRoleIdsOfAuthorizableItem("location-1").ToList());
            CollectionAssert.AreEquivalent(new List<string>() { "location-1", "location-2" }, persistence.GetAuthorizableItemIdsOfRole(roleA.Id).ToList());
            //removing an object removes its assignments as well, but not the assignments of other objects.
            persistence.RemoveInventoryObject("location-1");
            Assert.IsEmpty(persistence.GetRoleIdsOfAuthorizableItem("location-1"));
            CollectionAssert.AreEquivalent(new List<string>() { roleA.Id }, persistence.GetRoleIdsOfAuthorizableItem("location-2").ToList());
        }

        public abstract void SearchInventoryObjectsTest();
        public void SearchInventoryObjects()
        {
            ITimeService timeService = new TimeService();
            using PersistenceDisposable persistenceDisposable = this.GetPersistence(timeService);
            IPersistence persistence = persistenceDisposable.Persistence;

            //arrange
            persistence.AddInventoryObject(new InventoryObjectRecord("location-1", InventoryObjectType.Location, "Main site", string.Empty, null, null));
            persistence.AddInventoryObject(new InventoryObjectRecord("area-1", InventoryObjectType.Area, "Store room", string.Empty, "location-1", null));
            persistence.AddInventoryObject(new InventoryObjectRecord("container-1", InventoryObjectType.Container, "Big box", string.Empty, "area-1", null));
            persistence.AddInventoryObject(new InventoryObjectRecord("container-2", InventoryObjectType.Container, "small box", string.Empty, "area-1", null));
            persistence.AddInventoryObject(new InventoryObjectRecord("container-3", InventoryObjectType.Container, "Box_100%", string.Empty, "area-1", null));
            persistence.AddInventoryObject(new InventoryObjectRecord("item-1", InventoryObjectType.Item, "Box-cutter", string.Empty, "area-1", "area-1"));
            ISet<InventoryObjectType> containersAndAreas = new HashSet<InventoryObjectType>() { InventoryObjectType.Container, InventoryObjectType.Area };

            //act + assert
            CollectionAssert.AreEquivalent(new List<string>() { "container-1", "container-2", "container-3" }, persistence.SearchInventoryObjects("BOX", containersAndAreas, 10).Select(result => result.Id).ToList(), "The search ignores the case and only returns the requested types.");
            CollectionAssert.AreEquivalent(new List<string>() { "container-3" }, persistence.SearchInventoryObjects("_100%", containersAndAreas, 10).Select(result => result.Id).ToList(), "Wildcard-characters of like are taken literally.");
            CollectionAssert.AreEquivalent(new List<string>() { "area-1", "container-1", "container-2", "container-3" }, persistence.SearchInventoryObjects(string.Empty, containersAndAreas, 10).Select(result => result.Id).ToList(), "An empty text matches every name.");
            Assert.HasCount(2, persistence.SearchInventoryObjects("box", containersAndAreas, 2), "The amount of results is limited.");
            Assert.IsEmpty(persistence.SearchInventoryObjects("does not exist", containersAndAreas, 10));
            InventoryObjectRecord found = persistence.SearchInventoryObjects("cutter", new HashSet<InventoryObjectType>() { InventoryObjectType.Item }, 10).Single();
            Assert.AreEqual("Box-cutter", found.Name);
            Assert.AreEqual(InventoryObjectType.Item, found.Type);
        }

        public abstract void DeleteRoleTest();
        public void DeleteRole()
        {
            ITimeService timeService = new TimeService();
            using PersistenceDisposable persistenceDisposable = this.GetPersistence(timeService);
            IPersistence persistence = persistenceDisposable.Persistence;

            //arrange
            Core.Model.BusinessTypes.User user = CreateUser("judy", timeService);
            persistence.AddUser(user);
            Role role = new Role() { Id = Guid.NewGuid().ToString(), Name = "Warehouse", DirectlyInheritedRoles = new HashSet<Role>() };
            persistence.AddRole(role);
            persistence.AddRoleToUser(user.Id, role.Id);
            persistence.AddInventoryObject(new InventoryObjectRecord("location-1", InventoryObjectType.Location, "Location", string.Empty, null, null));
            persistence.AddRoleToAuthorizableItem(role.Id, "location-1");

            //act
            persistence.DeleteRoleByName(role.Name);

            //assert
            Assert.IsFalse(persistence.RoleExists(role.Name));
            Assert.IsFalse(persistence.GetUserById(user.Id).Roles.Any(roleOfUser => roleOfUser.Id == role.Id), "The members lose the deleted role.");
            Assert.IsEmpty(persistence.GetRoleIdsOfAuthorizableItem("location-1"));
            Assert.IsTrue(persistence.InventoryObjectExists("location-1"), "Deleting a role must not delete the objects it was assigned to.");
        }
    }
}
