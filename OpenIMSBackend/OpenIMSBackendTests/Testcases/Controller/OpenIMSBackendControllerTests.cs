using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.Misc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenIMSBackend.Core.Controller;
using OpenIMSBackend.Core.Model.BusinessTypes;
using OpenIMSBackend.Core.Model.DTOs;
using OpenIMSBackend.Core.Services;
using OpenIMSBackend.Tests.TestUtilities;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace OpenIMSBackend.Tests.Testcases.Controller
{
    /// <summary>
    /// Verifies the endpoints for modeling the inventory: that they translate http correctly into the business-logic
    /// and that they are not reachable without an access-token. The rules of the hierarchy themselves are covered by
    /// <see cref="Services.BusinessLogicServiceInventoryTests"/>.
    /// </summary>
    [TestClass]
    public class OpenIMSBackendControllerTests
    {
        private static string GetOpenIMSBackendControllerUrl(IntegrationTestFramework testFramework, string endpoint)
        {
            return $"{testFramework.GetServerURL()}/API/v{Core.Constants.GeneralConstants.CodeUnitMajorVersion}/OpenIMSBackend/{endpoint}";
        }

        [TestMethod(DisplayName = nameof(CreatedItemIsActuallyAtItsTargetPlace))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void CreatedItemIsActuallyAtItsTargetPlace()
        {
            //arrange
            (IBusinessLogicService businessLogicService, _, _, IAuthenticationService<User> authenticationService) = Services.BusinessLogicServiceTests.CreateSubjectUnderTest();
            OpenIMSBackendController controller = new OpenIMSBackendController(businessLogicService, authenticationService);
            string locationId = businessLogicService.CreateLocation("Location", string.Empty);
            string areaId = businessLogicService.CreateArea(locationId, "Area", string.Empty);
            string boxId = businessLogicService.CreateContainer(areaId, "Box", string.Empty);

            //act
            IActionResult result = controller.CreateItem(boxId, new InventoryObjectDataDTO("Drill", "Cordless drill"));

            //assert
            string itemId = ((StringValueDTO)((OkObjectResult)result).Value!).Value;
            Container box = (Container)businessLogicService.GetLocation(locationId).Areas.Single().Containees.Single();
            Item item = (Item)box.Containees.Single();
            Assert.AreEqual(itemId, item.Id);
            Assert.AreEqual("Drill", item.Name);
            Assert.AreEqual("Cordless drill", item.Description);
            Assert.AreSame(box, item.RealPlace, "A new item is where it belongs to.");
        }

        [TestMethod(DisplayName = nameof(GetInventoryTreeRequiresAnAccessToken))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void GetInventoryTreeRequiresAnAccessToken()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                //arrange
                using IntegrationTestFramework testFramework = new IntegrationTestFramework(new IntegrationTestConfiguration(null, true), true);
                using HttpClient client = testFramework.GetClient();

                //act
                HttpResponseMessage response = client.GetAsync(GetOpenIMSBackendControllerUrl(testFramework, "GetInventoryTree")).WaitAndGetResult();

                //assert
                Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        [TestMethod(DisplayName = nameof(CreateLocationRequiresAnAccessToken))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void CreateLocationRequiresAnAccessToken()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                //arrange
                using IntegrationTestFramework testFramework = new IntegrationTestFramework(new IntegrationTestConfiguration(null, true), true);
                using HttpClient client = testFramework.GetClient();
                using StringContent content = new StringContent("{\"name\":\"Location\",\"description\":\"\"}", Encoding.UTF8, "application/json");

                //act
                HttpResponseMessage response = client.PostAsync(GetOpenIMSBackendControllerUrl(testFramework, "CreateLocation"), content).WaitAndGetResult();

                //assert
                Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }
    }
}
