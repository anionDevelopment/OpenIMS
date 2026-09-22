using GRYLibrary.Core.APIServer.CommonDBTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenIMSBackend.Core.Constants;
using OpenIMSBackend.Core.Model.DTOs;
using System.Collections.Generic;

namespace OpenIMSBackend.Tests.Testcases.Miscellaneous
{
    [TestClass]
    public class UtilitiesTests
    {
        [TestMethod(DisplayName = nameof(GetUserInformationMarksAnAdministratorAsAdministrator))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void GetUserInformationMarksAnAdministratorAsAdministrator()
        {
            //arrange
            User user = new User() { Id = "some-id", Name = "some-name" };
            user.Roles = new HashSet<Role>() { new Role() { Id = "role-id", Name = CodeUnitSpecificConstants.RolenameAdmins, DirectlyInheritedRoles = new HashSet<Role>() } };

            //act
            UserInformationDTO actual = OpenIMSBackend.Core.Misc.Utilities.GetUserInformation(user);

            //assert
            Assert.AreEqual("some-id", actual.Id);
            Assert.AreEqual("some-name", actual.Name);
            Assert.IsTrue(actual.IsAdmin);
        }

        [TestMethod(DisplayName = nameof(GetUserInformationDoesNotMarkANormalUserAsAdministrator))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void GetUserInformationDoesNotMarkANormalUserAsAdministrator()
        {
            //arrange
            User user = new User() { Id = "some-id", Name = "some-name" };
            user.Roles = new HashSet<Role>() { new Role() { Id = "role-id", Name = CodeUnitSpecificConstants.RolenameUsers, DirectlyInheritedRoles = new HashSet<Role>() } };

            //act
            UserInformationDTO actual = OpenIMSBackend.Core.Misc.Utilities.GetUserInformation(user);

            //assert
            Assert.IsFalse(actual.IsAdmin);
        }

        [TestMethod(DisplayName = nameof(UserNameRegexAcceptsAValidNameAndRejectsAnInvalidOne))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void UserNameRegexAcceptsAValidNameAndRejectsAnInvalidOne()
        {
            Assert.IsTrue(CodeUnitSpecificConstants.UserNameRegex().IsMatch("alice"));
            Assert.IsTrue(CodeUnitSpecificConstants.UserNameRegex().IsMatch("user01"));
            Assert.IsFalse(CodeUnitSpecificConstants.UserNameRegex().IsMatch("01user"));
            Assert.IsFalse(CodeUnitSpecificConstants.UserNameRegex().IsMatch("has space"));
            Assert.IsFalse(CodeUnitSpecificConstants.UserNameRegex().IsMatch(string.Empty));
        }
    }
}
