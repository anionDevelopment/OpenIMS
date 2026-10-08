using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenIMSBackend.Core.Configuration;
using OpenIMSBackend.Core.Constants;
using OpenIMSBackend.Core.Model.DTOs;
using OpenIMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenIMSBackend.Tests.Testcases.Services
{
    /// <summary>
    /// Tests the business-logic against the transient persistence, so that the rules themselves are verified
    /// without a database being involved.
    /// </summary>
    [TestClass]
    public class BusinessLogicServiceTests
    {
        /// <summary>
        /// Builds a business-logic-service with a transient persistence and the roles which the initialization
        /// would create, so that a testcase can register a user without starting the whole server.
        /// </summary>
        internal static (IBusinessLogicService businessLogicService, IPersistence persistence, CodeUnitSpecificConfiguration configuration, IAuthenticationService<Core.Model.BusinessTypes.User> authenticationService) CreateSubjectUnderTest()
        {
            ITimeService timeService = new TimeService();
            TransientAuthenticationServicePersistence<Core.Model.BusinessTypes.User> authenticationPersistence = new TransientAuthenticationServicePersistence<Core.Model.BusinessTypes.User>(timeService);
            IPersistence persistence = new TransientPersistence(authenticationPersistence);

            Mock<IApplicationConstants<CodeUnitSpecificConstants>> applicationConstantsMock = new Mock<IApplicationConstants<CodeUnitSpecificConstants>>();
            applicationConstantsMock.Setup(m => m.Environment).Returns(QualityCheck.Instance);

            IAuthenticationService<Core.Model.BusinessTypes.User> authenticationService = new PersistentAuthenticationService(timeService, authenticationPersistence, ServerLog.GetTransientLog(), applicationConstantsMock.Object);

            CodeUnitSpecificConfiguration configuration = new CodeUnitSpecificConfiguration()
            {
                RegistrationIsEnabled = true,
                LoginIsEnabled = true,
            };
            Mock<IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>> configurationMock = new Mock<IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>>();
            configurationMock.Setup(m => m.ApplicationSpecificConfiguration).Returns(configuration);

            Mock<IAuditLog> auditLogMock = new Mock<IAuditLog>();
            auditLogMock.Setup(m => m.Logger).Returns(GRYLibrary.Core.Logging.GRYLogger.GRYLog.Create());

            //the two roles which the initialization creates as well. Without them a registration can not assign the base-role.
            authenticationService.EnsureRoleExists(CodeUnitSpecificConstants.RolenameUsers);
            Role usersRole = authenticationService.GetRoleByName(CodeUnitSpecificConstants.RolenameUsers);
            authenticationService.EnsureRoleExists(CodeUnitSpecificConstants.RolenameAdmins);
            Role adminsRole = authenticationService.GetRoleByName(CodeUnitSpecificConstants.RolenameAdmins);
            adminsRole.DirectlyInheritedRoles = new HashSet<Role>() { usersRole };
            authenticationService.UpdateRole(adminsRole);

            IBusinessLogicService businessLogicService = new BusinessLogicService(persistence, authenticationService, timeService, ServerLog.GetTransientLog(), configurationMock.Object, auditLogMock.Object);
            return (businessLogicService, persistence, configuration, authenticationService);
        }

        [TestMethod(DisplayName = nameof(LoginWithCorrectCredentialsReturnsAnAccessToken))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void LoginWithCorrectCredentialsReturnsAnAccessToken()
        {
            //arrange
            (IBusinessLogicService businessLogicService, _, _, _) = CreateSubjectUnderTest();
            string userId = businessLogicService.Register("alice", "pa55w0rd");

            //act
            AccessToken accessToken = businessLogicService.Login("alice", "pa55w0rd");

            //assert
            Assert.IsNotNull(accessToken.Value);
            Assert.AreEqual(userId, accessToken.OwnerUserId);
            Assert.IsTrue(businessLogicService.AccessTokenIsValid(accessToken.Value));
        }

        [TestMethod(DisplayName = nameof(LoginWithWrongPasswordIsRejected))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void LoginWithWrongPasswordIsRejected()
        {
            //arrange
            (IBusinessLogicService businessLogicService, _, _, _) = CreateSubjectUnderTest();
            businessLogicService.Register("alice", "pa55w0rd");

            //act + assert
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.Login("alice", "wrong-password"));
        }

        [TestMethod(DisplayName = nameof(LoginWithUnknownUserIsRejectedTheSameWayAsAWrongPassword))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void LoginWithUnknownUserIsRejectedTheSameWayAsAWrongPassword()
        {
            //an answer which differs between "this user does not exist" and "this password is wrong" would let a
            //caller find out which usernames exist, so both cases have to be answered identically.

            //arrange
            (IBusinessLogicService businessLogicService, _, _, _) = CreateSubjectUnderTest();
            businessLogicService.Register("alice", "pa55w0rd");

            //act
            BadRequestException exceptionForWrongPassword = Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.Login("alice", "wrong-password"));
            BadRequestException exceptionForUnknownUser = Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.Login("this-user-does-not-exist", "wrong-password"));

            //assert
            Assert.AreEqual(exceptionForWrongPassword.Message, exceptionForUnknownUser.Message);
        }

        [TestMethod(DisplayName = nameof(LoginIsRejectedWhenTheLoginIsDisabled))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void LoginIsRejectedWhenTheLoginIsDisabled()
        {
            //arrange
            (IBusinessLogicService businessLogicService, _, CodeUnitSpecificConfiguration configuration, _) = CreateSubjectUnderTest();
            businessLogicService.Register("alice", "pa55w0rd");
            configuration.LoginIsEnabled = false;

            //act + assert
            Assert.ThrowsExactly<NotAuthorizedException>(() => businessLogicService.Login("alice", "pa55w0rd"));
        }

        [TestMethod(DisplayName = nameof(LogoutInvalidatesTheAccessToken))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void LogoutInvalidatesTheAccessToken()
        {
            //arrange
            (IBusinessLogicService businessLogicService, _, _, _) = CreateSubjectUnderTest();
            businessLogicService.Register("alice", "pa55w0rd");
            AccessToken accessToken = businessLogicService.Login("alice", "pa55w0rd");

            //act
            businessLogicService.Logout(accessToken.Value);

            //assert
            Assert.IsFalse(businessLogicService.AccessTokenIsValid(accessToken.Value));
        }

        [TestMethod(DisplayName = nameof(PasswordIsNotStoredInPlaintext))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void PasswordIsNotStoredInPlaintext()
        {
            //arrange
            (IBusinessLogicService businessLogicService, IPersistence persistence, _, _) = CreateSubjectUnderTest();
            string password = "pa55w0rd";

            //act
            string userId = businessLogicService.Register("alice", password);

            //assert
            string? storedPasswordHash = persistence.GetUserById(userId).PasswordHash;
            Assert.IsNotNull(storedPasswordHash);
            Assert.AreNotEqual(password, storedPasswordHash);
        }

        [TestMethod(DisplayName = nameof(RegisterAssignsTheBaseRoleOfAllUser))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void RegisterAssignsTheBaseRoleOfAllUser()
        {
            //arrange
            (IBusinessLogicService businessLogicService, IPersistence persistence, _, _) = CreateSubjectUnderTest();

            //act
            string userId = businessLogicService.Register("alice", "pa55w0rd");

            //assert
            Assert.IsTrue(persistence.GetUserById(userId).Roles.Any(role => role.Name == CodeUnitSpecificConstants.RolenameUsers));
        }

        [TestMethod(DisplayName = nameof(RegisterRejectsAnAlreadyTakenUsername))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void RegisterRejectsAnAlreadyTakenUsername()
        {
            //arrange
            (IBusinessLogicService businessLogicService, _, _, _) = CreateSubjectUnderTest();
            businessLogicService.Register("alice", "pa55w0rd");

            //act + assert
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.Register("alice", "another-password"));
        }

        [TestMethod(DisplayName = nameof(RegisterRejectsAnInvalidUsername))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void RegisterRejectsAnInvalidUsername()
        {
            //arrange
            (IBusinessLogicService businessLogicService, _, _, _) = CreateSubjectUnderTest();

            //act + assert
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.Register("1startsWithADigit", "pa55w0rd"));
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.Register("contains spaces", "pa55w0rd"));
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.Register(string.Empty, "pa55w0rd"));
        }

        [TestMethod(DisplayName = nameof(RegisterIsRejectedWhenTheRegistrationIsDisabled))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void RegisterIsRejectedWhenTheRegistrationIsDisabled()
        {
            //arrange
            (IBusinessLogicService businessLogicService, _, CodeUnitSpecificConfiguration configuration, _) = CreateSubjectUnderTest();
            configuration.RegistrationIsEnabled = false;

            //act + assert
            Assert.ThrowsExactly<NotAuthorizedException>(() => businessLogicService.Register("alice", "pa55w0rd"));
        }

        [TestMethod(DisplayName = nameof(OnlyAnAdministratorMayReadTheUserOverview))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void OnlyAnAdministratorMayReadTheUserOverview()
        {
            //arrange
            (IBusinessLogicService businessLogicService, _, _, IAuthenticationService<Core.Model.BusinessTypes.User> authenticationService) = CreateSubjectUnderTest();
            string normalUserId = businessLogicService.Register("alice", "pa55w0rd");
            string adminUserId = businessLogicService.Register("admin", "pa55w0rd");
            authenticationService.EnsureUserHasRole(adminUserId, authenticationService.GetRoleByName(CodeUnitSpecificConstants.RolenameAdmins).Id);

            //act + assert
            Assert.ThrowsExactly<NotAuthorizedException>(() => businessLogicService.GetAllUsersWithRoles(normalUserId).ToList());
            List<UserOverviewDTO> usersSeenByTheAdmin = businessLogicService.GetAllUsersWithRoles(adminUserId).ToList();
            Assert.AreEqual(2, usersSeenByTheAdmin.Count);
        }

        [TestMethod(DisplayName = nameof(SetRolesOfUserSetsExactlyTheGivenRoles))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SetRolesOfUserSetsExactlyTheGivenRoles()
        {
            //arrange
            (IBusinessLogicService businessLogicService, IPersistence persistence, _, IAuthenticationService<Core.Model.BusinessTypes.User> authenticationService) = CreateSubjectUnderTest();
            string targetUserId = businessLogicService.Register("alice", "pa55w0rd");
            string adminUserId = businessLogicService.Register("admin", "pa55w0rd");
            authenticationService.EnsureUserHasRole(adminUserId, authenticationService.GetRoleByName(CodeUnitSpecificConstants.RolenameAdmins).Id);

            //act
            businessLogicService.SetRolesOfUser(adminUserId, targetUserId, new HashSet<string>() { CodeUnitSpecificConstants.RolenameAdmins });

            //assert
            ISet<string> actualRoleNames = persistence.GetUserById(targetUserId).Roles.Select(role => role.Name).ToHashSet();
            Assert.IsTrue(actualRoleNames.Contains(CodeUnitSpecificConstants.RolenameAdmins));
            Assert.IsFalse(actualRoleNames.Contains(CodeUnitSpecificConstants.RolenameUsers), "A role which was not part of the given set has to be removed.");
            Assert.IsTrue(businessLogicService.UserIsAdministrator(targetUserId));
        }

        [TestMethod(DisplayName = nameof(SetRolesOfUserRejectsAnUnknownRoleWithoutChangingAnything))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SetRolesOfUserRejectsAnUnknownRoleWithoutChangingAnything()
        {
            //arrange
            (IBusinessLogicService businessLogicService, IPersistence persistence, _, IAuthenticationService<Core.Model.BusinessTypes.User> authenticationService) = CreateSubjectUnderTest();
            string targetUserId = businessLogicService.Register("alice", "pa55w0rd");
            string adminUserId = businessLogicService.Register("admin", "pa55w0rd");
            authenticationService.EnsureUserHasRole(adminUserId, authenticationService.GetRoleByName(CodeUnitSpecificConstants.RolenameAdmins).Id);

            //act
            //Which exception the resolution of an unknown role-name throws is not part of the contract and
            //differs between the persistence-implementations; what matters is that the input is rejected.
            Assert.Throws<Exception>(() => businessLogicService.SetRolesOfUser(adminUserId, targetUserId, new HashSet<string>() { "ThisRoleDoesNotExist" }));

            //assert
            Assert.IsTrue(persistence.GetUserById(targetUserId).Roles.Any(role => role.Name == CodeUnitSpecificConstants.RolenameUsers), "The roles of the user must not be changed when the input was rejected.");
        }

        [TestMethod(DisplayName = nameof(ThemeOfAUserWhoDidNotChooseOneIsTheDefault))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ThemeOfAUserWhoDidNotChooseOneIsTheDefault()
        {
            //arrange
            (IBusinessLogicService businessLogicService, _, _, _) = CreateSubjectUnderTest();
            string userId = businessLogicService.Register("alice", "pa55w0rd");

            //act
            string actualTheme = businessLogicService.GetThemeOfUser(userId);

            //assert
            Assert.AreEqual(CodeUnitSpecificConstants.ThemeSystem, actualTheme);
        }

        [TestMethod(DisplayName = nameof(ThemeOfAUserIsStored))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ThemeOfAUserIsStored()
        {
            //arrange
            (IBusinessLogicService businessLogicService, _, _, _) = CreateSubjectUnderTest();
            string userId = businessLogicService.Register("alice", "pa55w0rd");

            //act
            businessLogicService.SetThemeOfUser(userId, CodeUnitSpecificConstants.ThemeDark);

            //assert
            Assert.AreEqual(CodeUnitSpecificConstants.ThemeDark, businessLogicService.GetThemeOfUser(userId));
        }

        [TestMethod(DisplayName = nameof(AnInvalidThemeIsRejected))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void AnInvalidThemeIsRejected()
        {
            //arrange
            (IBusinessLogicService businessLogicService, _, _, _) = CreateSubjectUnderTest();
            string userId = businessLogicService.Register("alice", "pa55w0rd");

            //act + assert
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.SetThemeOfUser(userId, "chartreuse"));
        }
    }
}
