using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenIMSBackend.Core.Services;
using System.Collections.Generic;

namespace OpenIMSBackend.Tests.Testcases.Services
{
    [TestClass]
    public class ExampleDataCreatorTests
    {
        [TestMethod(DisplayName = nameof(AddExampleDataCreatesTheExampleUsers))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void AddExampleDataCreatesTheExampleUsers()
        {
            //arrange
            List<(string username, string password)> registeredUsers = new List<(string, string)>();
            Mock<IBusinessLogicService> businessLogicServiceMock = new Mock<IBusinessLogicService>();
            businessLogicServiceMock
                .Setup(m => m.Register(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string username, string password) =>
                {
                    registeredUsers.Add((username, password));
                    return username + "-id";
                });
            ExampleDataCreator exampleDataCreator = new ExampleDataCreator(businessLogicServiceMock.Object);

            //act
            exampleDataCreator.AddExampleData();

            //assert
            Assert.AreEqual(2, registeredUsers.Count);
            Assert.AreEqual("user01", registeredUsers[0].username);
            Assert.AreEqual("user02", registeredUsers[1].username);
            //the password of an example-user is its name, so that it can be used to log in without looking anything up.
            foreach ((string username, string password) registeredUser in registeredUsers)
            {
                Assert.AreEqual(registeredUser.username, registeredUser.password);
            }
        }
    }
}
