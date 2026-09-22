using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenIMSBackend.Tests.TestUtilities;
using System.Net;
using System.Net.Http;

namespace OpenIMSBackend.Tests.Testcases.Controller
{
    /// <summary>
    /// Verifies against a really running backend which routes are reachable without an access-token and which
    /// are not. That is what covers the routing and the authentication-middleware together, which a unit-test of
    /// the business-logic can not do.
    /// <para>
    /// These testcases deliberately do not log anybody in: a server which is started by a testcase runs in the
    /// execution-mode <c>TestRun</c> (the production-path is only entered with "--RealRun true"), and in that
    /// mode the initialization-service does not run. So no roles and no admin-user exist, which means no account
    /// can be created and therefore nobody can log in. What happens after a successful login is covered by the
    /// unit-tests of the business-logic instead (see <see cref="Services.BusinessLogicServiceTests"/>).
    /// </para>
    /// </summary>
    [TestClass]
    public class UserControllerTests
    {
        private static string GetUserControllerUrl(IntegrationTestFramework testFramework, string endpoint)
        {
            return $"{testFramework.GetServerURL()}/API/v{Core.Constants.GeneralConstants.CodeUnitMajorVersion}/UserController/{endpoint}";
        }

        [TestMethod(DisplayName = nameof(LoginIsReachableWithoutAnAccessToken))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void LoginIsReachableWithoutAnAccessToken()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                //arrange
                using IntegrationTestFramework testFramework = new IntegrationTestFramework(new IntegrationTestConfiguration(null, true), true);
                using HttpClient client = testFramework.GetClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, GetUserControllerUrl(testFramework, "Login"));
                request.Headers.Add("user", "this-user-does-not-exist");
                request.Headers.Add("password", "wrong-password");

                //act
                HttpResponseMessage response = client.SendAsync(request).WaitAndGetResult();

                //assert
                //A rejected credential is answered with 401 in every configuration. That the status-code is the
                //same regardless of the persistence is not free: the transient authentication-service rejects
                //with 400 and the persistent one with 401, and the business-logic translates that into one
                //answer. The answer carries no body (the exception-middleware does not expose the message), so
                //the status-code is what can be asserted here. That the route is reachable without a token at all
                //is shown by TokenIsValidIsReachableWithoutAnAccessToken, which gets a real answer without one.
                Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        [TestMethod(DisplayName = nameof(TokenIsValidIsReachableWithoutAnAccessToken))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void TokenIsValidIsReachableWithoutAnAccessToken()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                //arrange
                //A client uses this endpoint to find out whether the token it still holds from an earlier session
                //can be used at all, so it has to be reachable without a valid one.
                using IntegrationTestFramework testFramework = new IntegrationTestFramework(new IntegrationTestConfiguration(null, true), true);
                using HttpClient client = testFramework.GetClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, GetUserControllerUrl(testFramework, "TokenIsValid"));
                request.Headers.Add("accessToken", "this-token-does-not-exist");

                //act
                HttpResponseMessage response = client.SendAsync(request).WaitAndGetResult();

                //assert
                string content = response.Content.ReadAsStringAsync().WaitAndGetResult();
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"Response-body: \"{content}\"");
                Assert.AreEqual("false", content.Trim().ToLowerInvariant());
            }
        }

        [TestMethod(DisplayName = nameof(GetUserInformationRequiresAnAccessToken))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void GetUserInformationRequiresAnAccessToken()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                //arrange
                using IntegrationTestFramework testFramework = new IntegrationTestFramework(new IntegrationTestConfiguration(null, true), true);
                using HttpClient client = testFramework.GetClient();

                //act
                HttpResponseMessage response = client.GetAsync(GetUserControllerUrl(testFramework, "GetUserInformation")).WaitAndGetResult();

                //assert
                //This route is not listed as unauthenticated, so the authentication-middleware has to reject it.
                Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        [TestMethod(DisplayName = nameof(GetAllUsersRequiresAnAccessToken))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void GetAllUsersRequiresAnAccessToken()
        {
            lock (TestUtilities.Utilities.LockForTests)
            {
                //arrange
                using IntegrationTestFramework testFramework = new IntegrationTestFramework(new IntegrationTestConfiguration(null, true), true);
                using HttpClient client = testFramework.GetClient();

                //act
                HttpResponseMessage response = client.GetAsync(GetUserControllerUrl(testFramework, "GetAllUsers")).WaitAndGetResult();

                //assert
                Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }
    }
}
