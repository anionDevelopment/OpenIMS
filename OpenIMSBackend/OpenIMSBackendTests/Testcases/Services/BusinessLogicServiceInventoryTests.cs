using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenIMSBackend.Core.Constants;
using OpenIMSBackend.Core.Model.BusinessTypes;
using OpenIMSBackend.Core.Model.DTOs;
using OpenIMSBackend.Core.Services;
using System.Collections.Generic;
using System.Linq;

namespace OpenIMSBackend.Tests.Testcases.Services
{
    /// <summary>
    /// Tests the rules of the inventory-hierarchy (location -> building -> area -> container -> item and
    /// location -> area) against the transient persistence.
    /// </summary>
    [TestClass]
    public class BusinessLogicServiceInventoryTests
    {
        private static IBusinessLogicService CreateBusinessLogicService()
        {
            return BusinessLogicServiceTests.CreateSubjectUnderTest().businessLogicService;
        }

        [TestMethod(DisplayName = nameof(CreatedHierarchyIsReturnedAsTree))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void CreatedHierarchyIsReturnedAsTree()
        {
            //arrange
            IBusinessLogicService businessLogicService = CreateBusinessLogicService();

            //act
            string locationId = businessLogicService.CreateLocation("Headquarter", "The main site");
            string buildingId = businessLogicService.CreateBuilding(locationId, "Building A", string.Empty);
            string roomId = businessLogicService.CreateArea(buildingId, "Room 101", string.Empty);
            string cupboardId = businessLogicService.CreateContainer(roomId, "Cupboard", string.Empty);
            string boxId = businessLogicService.CreateContainer(cupboardId, "Euro-box", string.Empty);
            string drillId = businessLogicService.CreateItem(boxId, boxId, "Drill", "Cordless drill");
            string yardId = businessLogicService.CreateArea(locationId, "Yard", string.Empty);
            string shovelId = businessLogicService.CreateItem(yardId, yardId, "Shovel", string.Empty);

            //assert
            Location location = businessLogicService.GetLocation(locationId);
            Assert.AreEqual("Headquarter", location.Name);
            Assert.AreEqual("The main site", location.Description);
            Building building = location.Buildings.Single();
            Assert.AreEqual(buildingId, building.Id);
            Area room = building.Areas.Single();
            Assert.AreEqual(roomId, room.Id);
            Container cupboard = (Container)room.Containees.Single();
            Assert.AreEqual(cupboardId, cupboard.Id);
            Container box = (Container)cupboard.Containees.Single();
            Assert.AreEqual(boxId, box.Id);
            Item drill = (Item)box.Containees.Single();
            Assert.AreEqual(drillId, drill.Id);
            Assert.AreSame(box, drill.RealPlace);
            Area yard = location.Areas.Single();
            Assert.AreEqual(yardId, yard.Id);
            Assert.AreEqual(shovelId, yard.Containees.Single().Id);
        }

        [TestMethod(DisplayName = nameof(RealPlaceCanDifferFromTargetPlace))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void RealPlaceCanDifferFromTargetPlace()
        {
            //arrange
            IBusinessLogicService businessLogicService = CreateBusinessLogicService();
            string locationAId = businessLogicService.CreateLocation("Location A", string.Empty);
            string locationBId = businessLogicService.CreateLocation("Location B", string.Empty);
            string areaAId = businessLogicService.CreateArea(locationAId, "Area A", string.Empty);
            string areaBId = businessLogicService.CreateArea(locationBId, "Area B", string.Empty);
            string itemId = businessLogicService.CreateItem(areaAId, areaAId, "Laptop", string.Empty);

            //act
            businessLogicService.SetRealPlaceOfItem(itemId, areaBId);

            //assert
            Location locationA = businessLogicService.GetLocation(locationAId);
            Item item = (Item)locationA.Areas.Single().Containees.Single();
            Assert.AreEqual(areaBId, item.RealPlace.Id);
        }

        [TestMethod(DisplayName = nameof(MovedContaineeIsOnlyInTheTargetContainer))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void MovedContaineeIsOnlyInTheTargetContainer()
        {
            //arrange
            IBusinessLogicService businessLogicService = CreateBusinessLogicService();
            string locationId = businessLogicService.CreateLocation("Location", string.Empty);
            string areaId = businessLogicService.CreateArea(locationId, "Area", string.Empty);
            string sourceId = businessLogicService.CreateContainer(areaId, "Source", string.Empty);
            string targetId = businessLogicService.CreateContainer(areaId, "Target", string.Empty);
            string itemId = businessLogicService.CreateItem(sourceId, sourceId, "Item", string.Empty);

            //act
            businessLogicService.MoveContainee(itemId, targetId);

            //assert
            Area area = businessLogicService.GetLocation(locationId).Areas.Single();
            Container source = (Container)area.Containees.Single(containee => containee.Id == sourceId);
            Container target = (Container)area.Containees.Single(containee => containee.Id == targetId);
            Assert.IsEmpty(source.Containees);
            Assert.AreEqual(itemId, target.Containees.Single().Id);
        }

        [TestMethod(DisplayName = nameof(MovedAreaIsOnlyInTheTarget))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void MovedAreaIsOnlyInTheTarget()
        {
            //arrange
            IBusinessLogicService businessLogicService = CreateBusinessLogicService();
            string locationId = businessLogicService.CreateLocation("Location", string.Empty);
            string buildingId = businessLogicService.CreateBuilding(locationId, "Building", string.Empty);
            string areaId = businessLogicService.CreateArea(locationId, "Area", string.Empty);

            //act
            businessLogicService.MoveArea(areaId, buildingId);

            //assert
            Location location = businessLogicService.GetLocation(locationId);
            Assert.IsEmpty(location.Areas);
            Assert.AreEqual(areaId, location.Buildings.Single().Areas.Single().Id);
        }

        [TestMethod(DisplayName = nameof(ContainerCanNotBeMovedIntoItself))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ContainerCanNotBeMovedIntoItself()
        {
            //arrange
            IBusinessLogicService businessLogicService = CreateBusinessLogicService();
            string locationId = businessLogicService.CreateLocation("Location", string.Empty);
            string areaId = businessLogicService.CreateArea(locationId, "Area", string.Empty);
            string containerId = businessLogicService.CreateContainer(areaId, "Container", string.Empty);

            //act + assert
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.MoveContainee(containerId, containerId));
        }

        [TestMethod(DisplayName = nameof(ContainerCanNotBeMovedIntoAContainerItContains))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ContainerCanNotBeMovedIntoAContainerItContains()
        {
            //arrange
            IBusinessLogicService businessLogicService = CreateBusinessLogicService();
            string locationId = businessLogicService.CreateLocation("Location", string.Empty);
            string areaId = businessLogicService.CreateArea(locationId, "Area", string.Empty);
            string outerId = businessLogicService.CreateContainer(areaId, "Outer", string.Empty);
            string middleId = businessLogicService.CreateContainer(outerId, "Middle", string.Empty);
            string innerId = businessLogicService.CreateContainer(middleId, "Inner", string.Empty);

            //act + assert
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.MoveContainee(outerId, innerId));
            Container outer = (Container)businessLogicService.GetLocation(locationId).Areas.Single().Containees.Single();
            Assert.AreEqual(outerId, outer.Id, "A rejected move must not change anything.");
        }

        [TestMethod(DisplayName = nameof(HierarchyLevelsCanNotBeMixed))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void HierarchyLevelsCanNotBeMixed()
        {
            //arrange
            IBusinessLogicService businessLogicService = CreateBusinessLogicService();
            string locationId = businessLogicService.CreateLocation("Location", string.Empty);
            string buildingId = businessLogicService.CreateBuilding(locationId, "Building", string.Empty);
            string areaId = businessLogicService.CreateArea(buildingId, "Area", string.Empty);
            string containerId = businessLogicService.CreateContainer(areaId, "Container", string.Empty);
            string itemId = businessLogicService.CreateItem(areaId, areaId, "Item", string.Empty);

            //act + assert
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.CreateBuilding(buildingId, "Building in a building", string.Empty), "A building can only be in a location.");
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.CreateArea(areaId, "Area in an area", string.Empty), "An area can only be in a location or a building.");
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.CreateArea(containerId, "Area in a container", string.Empty), "An area can only be in a location or a building.");
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.CreateContainer(locationId, "Container in a location", string.Empty), "A container can only be in an area or a container.");
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.CreateContainer(buildingId, "Container in a building", string.Empty), "A container can only be in an area or a container.");
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.CreateItem(buildingId, areaId, "Item in a building", string.Empty), "An item can only be in an area or a container.");
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.CreateItem(itemId, areaId, "Item in an item", string.Empty), "An item can not contain anything.");
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.CreateItem(areaId, itemId, "Item at an item", string.Empty), "An item is no real place.");
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.CreateItem(areaId, buildingId, "Item at a building", string.Empty), "A building is no real place.");
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.MoveContainee(areaId, containerId), "An area is no containee.");
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.MoveArea(areaId, containerId), "An area can only be in a location or a building.");
        }

        [TestMethod(DisplayName = nameof(UnknownIdIsAnsweredWithNotFound))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void UnknownIdIsAnsweredWithNotFound()
        {
            //arrange
            IBusinessLogicService businessLogicService = CreateBusinessLogicService();

            //act
            BadRequestException exception = Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.CreateBuilding("does-not-exist", "Building", string.Empty));

            //assert
            Assert.AreEqual(StatusCodes.Status404NotFound, exception.HTTPStatusCode);
        }

        [TestMethod(DisplayName = nameof(EmptyNameIsRejected))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void EmptyNameIsRejected()
        {
            //arrange
            IBusinessLogicService businessLogicService = CreateBusinessLogicService();
            string locationId = businessLogicService.CreateLocation("Location", string.Empty);

            //act + assert
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.CreateLocation(" ", string.Empty));
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.UpdateInventoryObject(locationId, string.Empty, string.Empty));
        }

        [TestMethod(DisplayName = nameof(UpdateChangesNameAndDescription))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void UpdateChangesNameAndDescription()
        {
            //arrange
            IBusinessLogicService businessLogicService = CreateBusinessLogicService();
            string locationId = businessLogicService.CreateLocation("Old name", "Old description");

            //act
            businessLogicService.UpdateInventoryObject(locationId, "New name", "New description");

            //assert
            Location location = businessLogicService.GetLocation(locationId);
            Assert.AreEqual("New name", location.Name);
            Assert.AreEqual("New description", location.Description);
        }

        [TestMethod(DisplayName = nameof(MovedBuildingBelongsToTheTargetLocation))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void MovedBuildingBelongsToTheTargetLocation()
        {
            //arrange
            IBusinessLogicService businessLogicService = CreateBusinessLogicService();
            string sourceLocationId = businessLogicService.CreateLocation("Source", string.Empty);
            string targetLocationId = businessLogicService.CreateLocation("Target", string.Empty);
            string buildingId = businessLogicService.CreateBuilding(sourceLocationId, "Building", string.Empty);

            //act
            businessLogicService.MoveBuilding(buildingId, targetLocationId);

            //assert
            Assert.IsEmpty(businessLogicService.GetLocation(sourceLocationId).Buildings);
            Assert.AreEqual(buildingId, businessLogicService.GetLocation(targetLocationId).Buildings.Single().Id);
        }

        [TestMethod(DisplayName = nameof(OnlyEmptyAndUnusedObjectCanBeDeleted))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void OnlyEmptyAndUnusedObjectCanBeDeleted()
        {
            //arrange
            IBusinessLogicService businessLogicService = CreateBusinessLogicService();
            string locationId = businessLogicService.CreateLocation("Location", string.Empty);
            string buildingId = businessLogicService.CreateBuilding(locationId, "Building", string.Empty);
            string areaId = businessLogicService.CreateArea(buildingId, "Area", string.Empty);
            string containerId = businessLogicService.CreateContainer(areaId, "Container", string.Empty);
            string itemId = businessLogicService.CreateItem(areaId, containerId, "Item", string.Empty);

            //act + assert
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.DeleteInventoryObject(buildingId), "The building still contains objects.");
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.DeleteInventoryObject(containerId), "The item actually is in the container.");

            businessLogicService.DeleteInventoryObject(itemId);
            businessLogicService.DeleteInventoryObject(containerId);
            businessLogicService.DeleteInventoryObject(areaId);
            businessLogicService.DeleteInventoryObject(buildingId);
            businessLogicService.DeleteInventoryObject(locationId);
            Assert.IsEmpty(businessLogicService.GetAllLocations());
        }

        [TestMethod(DisplayName = nameof(SearchReturnsIdAndLabelOfTheRequestedTypes))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SearchReturnsIdAndLabelOfTheRequestedTypes()
        {
            //arrange
            (IBusinessLogicService businessLogicService, _, _, IAuthenticationService<Core.Model.BusinessTypes.User> authenticationService) = BusinessLogicServiceTests.CreateSubjectUnderTest();
            string adminId = businessLogicService.Register("admin", "pa55w0rd");
            authenticationService.EnsureUserHasRole(adminId, authenticationService.GetRoleByName(CodeUnitSpecificConstants.RolenameAdmins).Id);
            string locationId = businessLogicService.CreateLocation("Toolbox site", string.Empty);
            string areaId = businessLogicService.CreateArea(locationId, "Area", string.Empty);
            string toolboxId = businessLogicService.CreateContainer(areaId, "Red toolbox", string.Empty);
            businessLogicService.CreateItem(toolboxId, toolboxId, "Toolbox key", string.Empty);

            //act
            IList<IdLabelDTO> result = businessLogicService.SearchInventoryObjects(adminId, "toolbox", new HashSet<string>() { "Container", "Location" });

            //assert
            CollectionAssert.AreEqual(new List<string>() { "Red toolbox", "Toolbox site" }, result.Select(entry => entry.Label).ToList());
            CollectionAssert.AreEqual(new List<string>() { toolboxId, locationId }, result.Select(entry => entry.Id).ToList());
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.SearchInventoryObjects(adminId, "toolbox", new HashSet<string>() { "Cupboard" }), "An unknown type is rejected.");
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.SearchInventoryObjects(adminId, "toolbox", new HashSet<string>()), "At least one type is required.");
        }

        [TestMethod(DisplayName = nameof(SearchIsLimited))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SearchIsLimited()
        {
            //arrange
            (IBusinessLogicService businessLogicService, _, _, IAuthenticationService<Core.Model.BusinessTypes.User> authenticationService) = BusinessLogicServiceTests.CreateSubjectUnderTest();
            string adminId = businessLogicService.Register("admin", "pa55w0rd");
            authenticationService.EnsureUserHasRole(adminId, authenticationService.GetRoleByName(CodeUnitSpecificConstants.RolenameAdmins).Id);
            string userId = businessLogicService.Register("alice", "pa55w0rd");
            for (int i = 0; i < BusinessLogicService.MaximalAmountOfSearchResults + 5; i++)
            {
                businessLogicService.CreateLocation($"Location {i:D2}", string.Empty);
            }

            //act + assert
            Assert.HasCount(BusinessLogicService.MaximalAmountOfSearchResults, businessLogicService.SearchInventoryObjects(adminId, string.Empty, new HashSet<string>() { "Location" }));
            Assert.ThrowsExactly<NotAuthorizedException>(() => businessLogicService.SearchInventoryObjects(userId, string.Empty, new HashSet<string>() { "Location" }), "The result is not restricted to the accessible objects, so only an administrator may search.");
        }

        [TestMethod(DisplayName = nameof(InventoryTreeContainsTheCompleteHierarchy))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void InventoryTreeContainsTheCompleteHierarchy()
        {
            //arrange
            IBusinessLogicService businessLogicService = CreateBusinessLogicService();
            string locationId = businessLogicService.CreateLocation("Headquarter", "The main site");
            string buildingId = businessLogicService.CreateBuilding(locationId, "Building A", "Office-building");
            string roomId = businessLogicService.CreateArea(buildingId, "Room 101", string.Empty);
            string boxId = businessLogicService.CreateContainer(roomId, "Euro-box", string.Empty);
            string drillId = businessLogicService.CreateItem(boxId, boxId, "Drill", "Cordless drill");
            string yardId = businessLogicService.CreateArea(locationId, "Yard", string.Empty);

            //act
            IList<InventoryTreeNodeDTO> tree = businessLogicService.GetInventoryTree();

            //assert
            InventoryTreeNodeDTO location = tree.Single();
            AssertNode(location, locationId, "Location", "Headquarter", "The main site");
            CollectionAssert.AreEqual(new List<string>() { buildingId, yardId }, location.Children.Select(child => child.Id).ToList(), "The buildings of a location are listed before its areas.");
            InventoryTreeNodeDTO building = location.Children[0];
            AssertNode(building, buildingId, "Building", "Building A", "Office-building");
            AssertNode(location.Children[1], yardId, "Area", "Yard", string.Empty);
            InventoryTreeNodeDTO room = building.Children.Single();
            AssertNode(room, roomId, "Area", "Room 101", string.Empty);
            InventoryTreeNodeDTO box = room.Children.Single();
            AssertNode(box, boxId, "Container", "Euro-box", string.Empty);
            InventoryTreeNodeDTO drill = box.Children.Single();
            AssertNode(drill, drillId, "Item", "Drill", "Cordless drill");
            Assert.IsEmpty(drill.Children);
        }

        [TestMethod(DisplayName = nameof(InventoryTreeOfAnEmptyInventoryIsEmpty))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void InventoryTreeOfAnEmptyInventoryIsEmpty()
        {
            //arrange
            IBusinessLogicService businessLogicService = CreateBusinessLogicService();

            //act
            IList<InventoryTreeNodeDTO> tree = businessLogicService.GetInventoryTree();

            //assert
            Assert.IsEmpty(tree);
        }

        [TestMethod(DisplayName = nameof(InventoryTreeListsAnItemOnlyAtItsTargetPlace))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void InventoryTreeListsAnItemOnlyAtItsTargetPlace()
        {
            //arrange
            IBusinessLogicService businessLogicService = CreateBusinessLogicService();
            string locationId = businessLogicService.CreateLocation("Location", string.Empty);
            string areaAId = businessLogicService.CreateArea(locationId, "Area A", string.Empty);
            string areaBId = businessLogicService.CreateArea(locationId, "Area B", string.Empty);
            string itemId = businessLogicService.CreateItem(areaAId, areaAId, "Laptop", string.Empty);
            businessLogicService.SetRealPlaceOfItem(itemId, areaBId);

            //act
            IList<InventoryTreeNodeDTO> tree = businessLogicService.GetInventoryTree();

            //assert
            IList<InventoryTreeNodeDTO> areas = tree.Single().Children;
            Assert.AreEqual(itemId, areas.Single(area => area.Id == areaAId).Children.Single().Id);
            Assert.IsEmpty(areas.Single(area => area.Id == areaBId).Children, "The real place of an item does not contain it in the tree.");
        }

        private static void AssertNode(InventoryTreeNodeDTO node, string expectedId, string expectedType, string expectedName, string expectedDescription)
        {
            Assert.AreEqual(expectedId, node.Id);
            Assert.AreEqual(expectedType, node.Type);
            Assert.AreEqual(expectedName, node.Name);
            Assert.AreEqual(expectedDescription, node.Description);
        }
    }
}
