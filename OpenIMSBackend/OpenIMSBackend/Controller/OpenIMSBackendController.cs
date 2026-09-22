using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.APIServer.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenIMSBackend.Core.Constants;
using OpenIMSBackend.Core.Model.DTOs;
using OpenIMSBackend.Core.Services;

namespace OpenIMSBackend.Core.Controller
{
    /// <summary>
    /// The controller of the business-functionality of OpenIMS. Everything which is not about the account of the
    /// caller itself (that is <see cref="UserController"/>) belongs here.
    /// </summary>
    [ApiController]
    [Route(ControllerRoute)]
    public class OpenIMSBackendController : ControllerBase
    {
        public const string ControllerRoute = $"{ServerConfiguration.APIRoutePrefix}/v{GeneralConstants.CodeUnitMajorVersion}/{GeneralConstants.CodeUnitName}";

        private readonly IBusinessLogicService _BusinessLogicService;
        private readonly IAuthenticationService _AuthenticationService;

        /// <summary>Initializes a new instance of <see cref="OpenIMSBackendController"/>.</summary>
        /// <param name="businessLogicService">The business logic service which processes the operations.</param>
        /// <param name="authenticationService">The authentication service used to resolve the current user.</param>
        public OpenIMSBackendController(IBusinessLogicService businessLogicService, IAuthenticationService authenticationService)
        {
            this._BusinessLogicService = businessLogicService;
            this._AuthenticationService = authenticationService;
        }

        /// <summary>Returns the general settings of this installation. Requires administrator privileges.</summary>
        /// <returns>The current <see cref="GeneralSettingsDTO"/>.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameAdmins)]
        [HttpGet]
        [ProducesResponseType(typeof(GeneralSettingsDTO), StatusCodes.Status200OK)]
        [Route(nameof(GetGeneralSettings))]
        public IActionResult GetGeneralSettings()
        {
            return this.Ok(new GeneralSettingsDTO(this._BusinessLogicService.GetRegistrationIsEnabled()));
        }

        /// <summary>Changes the general settings of this installation. Requires administrator privileges.</summary>
        /// <param name="settings">The settings to apply.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameAdmins)]
        [HttpPut]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [Route(nameof(SetGeneralSettings))]
        public IActionResult SetGeneralSettings([FromBody] GeneralSettingsDTO settings)
        {
            this._BusinessLogicService.SetRegistrationIsEnabled(this.GetUser().Id, settings.RegistrationIsEnabled);
            return this.Ok();
        }

        private User GetUser()
        {
            return Tools.GetUser(this.User, this._AuthenticationService);
        }
    }
}
