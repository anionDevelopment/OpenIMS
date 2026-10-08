using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenIMSBackend.Core.Constants;
using OpenIMSBackend.Core.Model.DTOs;
using OpenIMSBackend.Core.Services;
using System.Collections.Generic;
using System.Linq;

namespace OpenIMSBackend.Tests.Testcases.Services
{
    /// <summary>
    /// Tests the assignment of groups to authorizable items and the access which is inherited at runtime from a
    /// containing object, against the transient persistence.
    /// </summary>
    [TestClass]
    public class BusinessLogicServiceAuthorizationTests
    {
        private const string WarehouseGroupName = "Warehouse";

        /// <summary>
        /// Creates the subject under test with an administrator, a member of the group <see cref="WarehouseGroupName"/>
        /// and a user who only has the base-role, plus the hierarchy location -> building -> area -> container -> item.
        /// </summary>
        private static Scenario CreateScenario()
        {
            (IBusinessLogicService businessLogicService, _, _, IAuthenticationService<Core.Model.BusinessTypes.User> authenticationService) = BusinessLogicServiceTests.CreateSubjectUnderTest();
            authenticationService.EnsureRoleExists(WarehouseGroupName);

            string adminId = businessLogicService.Register("admin", "pa55w0rd");
            authenticationService.EnsureUserHasRole(adminId, authenticationService.GetRoleByName(CodeUnitSpecificConstants.RolenameAdmins).Id);
            string warehouseMemberId = businessLogicService.Register("warehousemember", "pa55w0rd");
            authenticationService.EnsureUserHasRole(warehouseMemberId, authenticationService.GetRoleByName(WarehouseGroupName).Id);
            string otherUserId = businessLogicService.Register("otheruser", "pa55w0rd");

            string locationId = businessLogicService.CreateLocation("Location", string.Empty);
            string buildingId = businessLogicService.CreateBuilding(locationId, "Building", string.Empty);
            string areaId = businessLogicService.CreateArea(buildingId, "Area", string.Empty);
            string containerId = businessLogicService.CreateContainer(areaId, "Container", string.Empty);
            string itemId = businessLogicService.CreateItem(containerId, containerId, "Item", string.Empty);
            return new Scenario(businessLogicService, adminId, warehouseMemberId, otherUserId, locationId, buildingId, areaId, containerId, itemId);
        }

        private sealed record Scenario(IBusinessLogicService BusinessLogicService, string AdminId, string WarehouseMemberId, string OtherUserId, string LocationId, string BuildingId, string AreaId, string ContainerId, string ItemId);

        [TestMethod(DisplayName = nameof(AccessIsInheritedByEverythingWhichIsContained))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void AccessIsInheritedByEverythingWhichIsContained()
        {
            //arrange
            Scenario scenario = CreateScenario();

            //act
            scenario.BusinessLogicService.AssignGroupToAuthorizableItem(scenario.AdminId, WarehouseGroupName, scenario.BuildingId);

            //assert
            IBusinessLogicService service = scenario.BusinessLogicService;
            Assert.IsFalse(service.UserHasAccess(scenario.WarehouseMemberId, scenario.LocationId), "The access must not be inherited upwards.");
            Assert.IsTrue(service.UserHasAccess(scenario.WarehouseMemberId, scenario.BuildingId));
            Assert.IsTrue(service.UserHasAccess(scenario.WarehouseMemberId, scenario.AreaId));
            Assert.IsTrue(service.UserHasAccess(scenario.WarehouseMemberId, scenario.ContainerId));
            Assert.IsTrue(service.UserHasAccess(scenario.WarehouseMemberId, scenario.ItemId));
            Assert.IsFalse(service.UserHasAccess(scenario.OtherUserId, scenario.ItemId), "A user who is not member of an assigned group must not have access.");
        }

        [TestMethod(DisplayName = nameof(OnlyTheDirectAssignmentIsStored))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void OnlyTheDirectAssignmentIsStored()
        {
            //arrange
            Scenario scenario = CreateScenario();

            //act
            scenario.BusinessLogicService.AssignGroupToAuthorizableItem(scenario.AdminId, WarehouseGroupName, scenario.AreaId);

            //assert
            CollectionAssert.AreEquivalent(new List<string>() { WarehouseGroupName }, scenario.BusinessLogicService.GetGroupsOfAuthorizableItem(scenario.AdminId, scenario.AreaId).ToList());
            Assert.IsEmpty(scenario.BusinessLogicService.GetGroupsOfAuthorizableItem(scenario.AdminId, scenario.ItemId), "The inherited access is computed and not stored.");
        }

        [TestMethod(DisplayName = nameof(AccessOfAnInheritedRoleApplies))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void AccessOfAnInheritedRoleApplies()
        {
            //arrange
            Scenario scenario = CreateScenario();

            //act
            scenario.BusinessLogicService.AssignGroupToAuthorizableItem(scenario.AdminId, CodeUnitSpecificConstants.RolenameUsers, scenario.AreaId);

            //assert
            Assert.IsTrue(scenario.BusinessLogicService.UserHasAccess(scenario.OtherUserId, scenario.ItemId));
            Assert.IsTrue(scenario.BusinessLogicService.UserHasAccess(scenario.AdminId, scenario.ItemId), "The role of the administrators inherits the role of all users.");
        }

        [TestMethod(DisplayName = nameof(RealPlaceDoesNotGrantAccess))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void RealPlaceDoesNotGrantAccess()
        {
            //arrange
            Scenario scenario = CreateScenario();
            IBusinessLogicService service = scenario.BusinessLogicService;
            string otherAreaId = service.CreateArea(scenario.LocationId, "Other area", string.Empty);
            service.AssignGroupToAuthorizableItem(scenario.AdminId, WarehouseGroupName, otherAreaId);

            //act
            service.SetRealPlaceOfItem(scenario.ItemId, otherAreaId);

            //assert
            Assert.IsFalse(service.UserHasAccess(scenario.WarehouseMemberId, scenario.ItemId), "Only the containment-hierarchy decides about the access, not the real place.");
        }

        [TestMethod(DisplayName = nameof(MovedObjectGetsTheAccessOfItsNewPlace))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void MovedObjectGetsTheAccessOfItsNewPlace()
        {
            //arrange
            Scenario scenario = CreateScenario();
            IBusinessLogicService service = scenario.BusinessLogicService;
            string otherAreaId = service.CreateArea(scenario.LocationId, "Other area", string.Empty);
            service.AssignGroupToAuthorizableItem(scenario.AdminId, WarehouseGroupName, scenario.AreaId);

            //act
            service.MoveContainee(scenario.ContainerId, otherAreaId);

            //assert
            Assert.IsFalse(service.UserHasAccess(scenario.WarehouseMemberId, scenario.ItemId), "The access is computed from the current hierarchy, so it follows a move.");
        }

        [TestMethod(DisplayName = nameof(RemovedAssignmentRevokesTheAccess))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void RemovedAssignmentRevokesTheAccess()
        {
            //arrange
            Scenario scenario = CreateScenario();
            IBusinessLogicService service = scenario.BusinessLogicService;
            service.AssignGroupToAuthorizableItem(scenario.AdminId, WarehouseGroupName, scenario.ContainerId);
            service.AssignGroupToAuthorizableItem(scenario.AdminId, WarehouseGroupName, scenario.ContainerId);

            //act
            service.RemoveGroupFromAuthorizableItem(scenario.AdminId, WarehouseGroupName, scenario.ContainerId);

            //assert
            Assert.IsFalse(service.UserHasAccess(scenario.WarehouseMemberId, scenario.ItemId));
            Assert.IsEmpty(service.GetGroupsOfAuthorizableItem(scenario.AdminId, scenario.ContainerId));
        }

        [TestMethod(DisplayName = nameof(OnlyAdministratorsCanAssignGroups))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void OnlyAdministratorsCanAssignGroups()
        {
            //arrange
            Scenario scenario = CreateScenario();

            //act + assert
            Assert.ThrowsExactly<NotAuthorizedException>(() => scenario.BusinessLogicService.AssignGroupToAuthorizableItem(scenario.WarehouseMemberId, WarehouseGroupName, scenario.ItemId));
            Assert.ThrowsExactly<NotAuthorizedException>(() => scenario.BusinessLogicService.GetGroupsOfAuthorizableItem(scenario.WarehouseMemberId, scenario.ItemId));
        }

        [TestMethod(DisplayName = nameof(UnknownGroupIsAnsweredWithNotFound))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void UnknownGroupIsAnsweredWithNotFound()
        {
            //arrange
            Scenario scenario = CreateScenario();

            //act
            BadRequestException exception = Assert.ThrowsExactly<BadRequestException>(() => scenario.BusinessLogicService.AssignGroupToAuthorizableItem(scenario.AdminId, "does-not-exist", scenario.ItemId));

            //assert
            Assert.AreEqual(StatusCodes.Status404NotFound, exception.HTTPStatusCode);
        }

        [TestMethod(DisplayName = nameof(ObjectWithAssignedGroupCanBeDeleted))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ObjectWithAssignedGroupCanBeDeleted()
        {
            //arrange
            Scenario scenario = CreateScenario();
            scenario.BusinessLogicService.AssignGroupToAuthorizableItem(scenario.AdminId, WarehouseGroupName, scenario.ItemId);

            //act
            scenario.BusinessLogicService.DeleteInventoryObject(scenario.ItemId);

            //assert
            Assert.ThrowsExactly<BadRequestException>(() => scenario.BusinessLogicService.UserHasAccess(scenario.WarehouseMemberId, scenario.ItemId));
        }

        [TestMethod(DisplayName = nameof(CreatedGroupCanBeAssigned))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void CreatedGroupCanBeAssigned()
        {
            //arrange
            Scenario scenario = CreateScenario();
            IBusinessLogicService service = scenario.BusinessLogicService;

            //act
            service.CreateGroup(scenario.AdminId, "Workshop");
            service.AssignGroupToAuthorizableItem(scenario.AdminId, "Workshop", scenario.ContainerId);
            service.AssignGroupToAuthorizableItem(scenario.AdminId, "Workshop", scenario.AreaId);

            //assert
            Assert.Contains("Workshop", service.GetAllRoleNames(scenario.AdminId).ToList());
            IList<IdLabelDTO> items = service.GetAuthorizableItemsOfGroup(scenario.AdminId, "Workshop");
            CollectionAssert.AreEqual(new List<string>() { scenario.AreaId, scenario.ContainerId }, items.Select(item => item.Id).ToList(), "The items are ordered by their label.");
            CollectionAssert.AreEqual(new List<string>() { "Area", "Container" }, items.Select(item => item.Label).ToList());
        }

        [TestMethod(DisplayName = nameof(InvalidGroupNameIsRejected))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void InvalidGroupNameIsRejected()
        {
            //arrange
            Scenario scenario = CreateScenario();

            //act + assert
            Assert.ThrowsExactly<BadRequestException>(() => scenario.BusinessLogicService.CreateGroup(scenario.AdminId, " "));
            Assert.ThrowsExactly<BadRequestException>(() => scenario.BusinessLogicService.CreateGroup(scenario.AdminId, " Workshop"));
            Assert.ThrowsExactly<BadRequestException>(() => scenario.BusinessLogicService.CreateGroup(scenario.AdminId, WarehouseGroupName), "The name is already used.");
            Assert.ThrowsExactly<NotAuthorizedException>(() => scenario.BusinessLogicService.CreateGroup(scenario.WarehouseMemberId, "Workshop"));
        }

        [TestMethod(DisplayName = nameof(DeletedGroupLosesMembersAndAssignments))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void DeletedGroupLosesMembersAndAssignments()
        {
            //arrange
            Scenario scenario = CreateScenario();
            IBusinessLogicService service = scenario.BusinessLogicService;
            service.AssignGroupToAuthorizableItem(scenario.AdminId, WarehouseGroupName, scenario.LocationId);

            //act
            service.DeleteGroup(scenario.AdminId, WarehouseGroupName);

            //assert
            Assert.DoesNotContain(WarehouseGroupName, service.GetAllRoleNames(scenario.AdminId).ToList());
            Assert.IsFalse(service.UserHasAccess(scenario.WarehouseMemberId, scenario.ItemId));
            Assert.IsEmpty(service.GetGroupsOfAuthorizableItem(scenario.AdminId, scenario.LocationId));
        }

        [TestMethod(DisplayName = nameof(RequiredGroupsCanNotBeDeleted))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void RequiredGroupsCanNotBeDeleted()
        {
            //arrange
            Scenario scenario = CreateScenario();

            //act + assert
            Assert.ThrowsExactly<BadRequestException>(() => scenario.BusinessLogicService.DeleteGroup(scenario.AdminId, CodeUnitSpecificConstants.RolenameUsers));
            Assert.ThrowsExactly<BadRequestException>(() => scenario.BusinessLogicService.DeleteGroup(scenario.AdminId, CodeUnitSpecificConstants.RolenameAdmins));
            Assert.ThrowsExactly<NotAuthorizedException>(() => scenario.BusinessLogicService.DeleteGroup(scenario.WarehouseMemberId, WarehouseGroupName));
        }
    }
}
