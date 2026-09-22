using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.MidT.Auth;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenIMSBackend.Core.Constants;
using OpenIMSBackend.Core.Misc;
using OpenIMSBackend.Core.Model.DTOs;
using OpenIMSBackend.Core.Services;
using System.Linq;
using IAuthenticationService = GRYLibrary.Core.APIServer.Services.Interfaces.IAuthenticationService;

namespace OpenIMSBackend.Core.Controller
{
    [ApiController]
    [Route(ControllerRoute)]
    public class UserController : ControllerBase
    {
        public const string ControllerRoute = $"{ServerConfiguration.APIRoutePrefix}/v{GeneralConstants.CodeUnitMajorVersion}/{nameof(UserController)}";
        private readonly IGeneralLogger _Logger;
        private readonly IAuthenticationService _AuthenticationService;
        private readonly IBusinessLogicService _BusinessLogicService;

        /// <summary>Initializes a new instance of <see cref="UserController"/>.</summary>
        /// <param name="logger">The logger for diagnostic output.</param>
        /// <param name="authenticationService">The authentication service which resolves the current user.</param>
        /// <param name="businessLogicService">The business logic service.</param>
        public UserController(IServerLog logger, IAuthenticationService authenticationService, IBusinessLogicService businessLogicService)
        {
            this._Logger = logger.Logger;
            this._AuthenticationService = authenticationService;
            this._BusinessLogicService = businessLogicService;
        }

        /// <summary>Authenticates a user with the given credentials and returns an access token on success.</summary>
        /// <param name="user">The username.</param>
        /// <param name="password">The plain-text password.</param>
        /// <returns>An <see cref="AccessToken"/> on success, or 401 if the credentials are invalid.</returns>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccessToken))]
        [Route(nameof(Login))]
        public IActionResult Login([FromHeader] string user, [FromHeader] string password)
        {
            return this.Ok(this._BusinessLogicService.Login(user, password));
        }

        /// <summary>Creates a new user-account. Requires administrator privileges.</summary>
        /// <param name="user">The username for the new account.</param>
        /// <param name="password">The plain-text password for the new account.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameAdmins)]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(void))]
        [Route(nameof(Register))]
        public IActionResult Register([FromHeader] string user, [FromHeader] string password)
        {
            this._BusinessLogicService.Register(user, password);
            return this.Ok();
        }

        /// <summary>Invalidates the access-token of the current session, logging the user out.</summary>
        /// <returns>200 OK on success, or 401 if no active token is present.</returns>
        [Authenticate]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(void))]
        [Route(nameof(Logout))]
        public IActionResult Logout()
        {
            if ((!this.HttpContext.Items.ContainsKey(AuthenticationMiddleware.CurrentlyUsedAccessTokenInformationName)) ||
                (this.HttpContext.Items[AuthenticationMiddleware.CurrentlyUsedAccessTokenInformationName] == null))
            {
                return this.StatusCode(StatusCodes.Status401Unauthorized);
            }
            string currentlyUsedAccessToken = (string)this.HttpContext.Items[AuthenticationMiddleware.CurrentlyUsedAccessTokenInformationName]!;
            this._BusinessLogicService.Logout(currentlyUsedAccessToken);
            return this.Ok();
        }

        /// <summary>Checks whether the given access-token is currently valid.</summary>
        /// <param name="accessToken">The access-token to validate.</param>
        /// <returns><see langword="true"/> if the token is valid; otherwise <see langword="false"/>.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [Route(nameof(TokenIsValid))]
        public IActionResult TokenIsValid([FromHeader] string accessToken)
        {
            return this.Ok(this._BusinessLogicService.AccessTokenIsValid(accessToken));
        }

        /// <summary>Returns the roles assigned to the currently authenticated user.</summary>
        /// <returns>An array of role-names.</returns>
        [Authenticate]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string[]))]
        [Route(nameof(GetRoles))]
        public IActionResult GetRoles()
        {
            return this.Ok(this.GetUser().Roles);
        }

        /// <summary>Returns profile-information for the currently authenticated user.</summary>
        /// <returns>A <see cref="UserInformationDTO"/> containing the id, the name and the admin-flag of the user.</returns>
        [Authenticate]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserInformationDTO))]
        [Route(nameof(GetUserInformation))]
        public IActionResult GetUserInformation()
        {
            return this.Ok(Utilities.GetUserInformation(this.GetUser()));
        }

        /// <summary>Returns the color-scheme which the currently authenticated user chose.</summary>
        /// <returns>A <see cref="StringValueDTO"/> containing "system", "light" or "dark". A user who did not choose a color-scheme yet gets "system", which follows the setting of the operating-system of that user.</returns>
        [Authenticate]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StringValueDTO))]
        [Route(nameof(GetTheme))]
        public IActionResult GetTheme()
        {
            return this.Ok(new StringValueDTO() { Value = this._BusinessLogicService.GetThemeOfUser(this.GetUser().Id) });
        }

        /// <summary>Sets the color-scheme of the currently authenticated user, so that the choice is available again on another device and after a new login.</summary>
        /// <param name="theme">The color-scheme to store: "system", "light" or "dark".</param>
        /// <returns>200 if the color-scheme was stored.</returns>
        [Authenticate]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route(nameof(SetTheme))]
        public IActionResult SetTheme([FromBody] StringValueDTO theme)
        {
            this._BusinessLogicService.SetThemeOfUser(this.GetUser().Id, theme.Value);
            return this.Ok();
        }

        /// <summary>Returns all users together with the roles assigned to them. Requires administrator privileges.</summary>
        /// <returns>An array of <see cref="UserOverviewDTO"/>.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameAdmins)]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserOverviewDTO[]))]
        [Route(nameof(GetAllUsers))]
        public IActionResult GetAllUsers()
        {
            return this.Ok(this._BusinessLogicService.GetAllUsersWithRoles(this.GetUser().Id));
        }

        /// <summary>Returns the names of all roles that can be assigned to a user. Requires administrator privileges.</summary>
        /// <returns>An array of role-names.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameAdmins)]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string[]))]
        [Route(nameof(GetAllRoles))]
        public IActionResult GetAllRoles()
        {
            return this.Ok(this._BusinessLogicService.GetAllRoleNames(this.GetUser().Id));
        }

        /// <summary>Sets the complete set of roles of the given user (roles not contained are removed, missing ones are added). Requires administrator privileges.</summary>
        /// <param name="userId">The id of the user whose roles should be set.</param>
        /// <param name="roleNames">The names of the roles the user should have afterwards.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameAdmins)]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(void))]
        [Route(nameof(SetRolesOfUser))]
        public IActionResult SetRolesOfUser([FromHeader] string userId, [FromBody] string[] roleNames)
        {
            this._BusinessLogicService.SetRolesOfUser(this.GetUser().Id, userId, roleNames.ToHashSet());
            return this.Ok();
        }

        private User GetUser()
        {
            return Tools.GetUser(this.User, this._AuthenticationService);
        }
    }
}
