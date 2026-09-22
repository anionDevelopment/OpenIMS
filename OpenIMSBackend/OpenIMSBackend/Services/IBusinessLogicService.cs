using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using OpenIMSBackend.Core.Model.BusinessTypes;
using OpenIMSBackend.Core.Model.DTOs;
using System.Collections.Generic;

namespace OpenIMSBackend.Core.Services
{
    /// <summary>
    /// The business-logic of OpenIMS. Everything a controller does goes through this interface, so that the
    /// rules (who is allowed to do what, what is a valid value) exist exactly once and are not spread over the
    /// endpoints.
    /// </summary>
    public interface IBusinessLogicService
    {
        #region Login

        /// <summary>Authenticates a user by its credentials.</summary>
        /// <param name="username">The name of the user.</param>
        /// <param name="password">The plain-text password of the user.</param>
        /// <returns>The access-token which the caller has to send in the header of every following authenticated request.</returns>
        /// <exception cref="GRYLibrary.Core.Exceptions.BadRequestException">Thrown with the status-code 401 when the user does not exist or the password is wrong. Both cases are answered identically on purpose, so that the answer does not reveal whether the username exists.</exception>
        /// <exception cref="GRYLibrary.Core.Exceptions.NotAuthorizedException">Thrown when the user exists and the password is correct but the account is locked.</exception>
        public AccessToken Login(string username, string password);

        /// <summary>Invalidates the given access-token, so that it can not be used anymore.</summary>
        /// <param name="accessToken">The access-token to invalidate.</param>
        public void Logout(string accessToken);

        /// <summary>Returns whether the given access-token exists and has not expired yet.</summary>
        /// <param name="accessToken">The access-token to check.</param>
        /// <returns><see langword="true"/> if the token can be used; otherwise <see langword="false"/>.</returns>
        public bool AccessTokenIsValid(string accessToken);

        #endregion

        #region User

        /// <summary>Creates a new user-account which has the role of every user.</summary>
        /// <param name="username">The name of the new account. It must be unique and must match <see cref="Constants.CodeUnitSpecificConstants.UserNameRegex"/>.</param>
        /// <param name="password">The plain-text password of the new account. Only its hash is stored.</param>
        /// <returns>The id of the created user.</returns>
        public string Register(string username, string password);

        /// <summary>Returns whether a user with the given name exists.</summary>
        public bool UserWithNameExists(string username);

        /// <summary>Returns whether the user with the given id has the role which identifies an administrator.</summary>
        public bool UserIsAdministrator(string userId);

        /// <summary>Returns the user with the given id.</summary>
        public User GetUser(string userId);

        /// <summary>Returns all users together with the names of the roles assigned to them. Only an administrator may do this.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        public IEnumerable<UserOverviewDTO> GetAllUsersWithRoles(string requesterUserId);

        /// <summary>Returns the names of all roles which can be assigned to a user. Only an administrator may do this.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        public IEnumerable<string> GetAllRoleNames(string requesterUserId);

        /// <summary>Sets the complete set of roles of the given user (roles not contained are removed, missing ones are added). Only an administrator may do this.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="targetUserId">The id of the user whose roles should be set.</param>
        /// <param name="roleNames">The names of the roles the user should have afterwards.</param>
        public void SetRolesOfUser(string requesterUserId, string targetUserId, ISet<string> roleNames);

        #endregion

        #region Settings

        /// <summary>Returns whether a new user-account can be created at all. This is the value an administrator set; as long as none was set it is the configuration-value.</summary>
        public bool GetRegistrationIsEnabled();

        /// <summary>Sets whether a new user-account can be created at all. Only an administrator may change this.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="enabled">Whether the creation of a new user-account should be possible.</param>
        public void SetRegistrationIsEnabled(string requesterUserId, bool enabled);

        /// <summary>Returns the color-scheme the given user chose.</summary>
        /// <param name="userId">The id of the user.</param>
        /// <returns>"system", "light" or "dark". A user who did not choose a color-scheme yet gets "system", which follows the setting of the operating-system of that user.</returns>
        public string GetThemeOfUser(string userId);

        /// <summary>Sets the color-scheme of the given user.</summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="theme">"system", "light" or "dark".</param>
        public void SetThemeOfUser(string userId, string theme);

        #endregion
    }
}
