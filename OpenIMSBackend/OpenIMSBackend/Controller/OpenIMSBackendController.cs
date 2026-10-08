using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.APIServer.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenIMSBackend.Core.Constants;
using OpenIMSBackend.Core.Model.DTOs;
using OpenIMSBackend.Core.Services;
using System.Linq;

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

        /// <summary>Searches the inventory-objects of the given types whose name contains the given text (ignoring the case). The amount of results is limited, so this is meant to be called while the user types. Requires administrator privileges.</summary>
        /// <param name="searchText">The text which the name has to contain. An empty text matches every name.</param>
        /// <param name="types">The types of the objects which are searched: "Location", "Building", "Area", "Container" and/or "Item".</param>
        /// <returns>An array of <see cref="IdLabelDTO"/>, ordered by the label.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameAdmins)]
        [HttpGet]
        [ProducesResponseType(typeof(IdLabelDTO[]), StatusCodes.Status200OK)]
        [Route(nameof(SearchInventoryObjects))]
        public IActionResult SearchInventoryObjects([FromQuery] string? searchText, [FromQuery] string[] types)
        {
            return this.Ok(this._BusinessLogicService.SearchInventoryObjects(this.GetUser().Id, searchText ?? string.Empty, types.ToHashSet()));
        }

        /// <summary>Returns the inventory-objects to which the given group is directly assigned. Requires administrator privileges.</summary>
        /// <param name="groupName">The name of the group.</param>
        /// <returns>An array of <see cref="IdLabelDTO"/>, ordered by the label.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameAdmins)]
        [HttpGet]
        [ProducesResponseType(typeof(IdLabelDTO[]), StatusCodes.Status200OK)]
        [Route(nameof(GetAuthorizableItemsOfGroup))]
        public IActionResult GetAuthorizableItemsOfGroup([FromQuery] string groupName)
        {
            return this.Ok(this._BusinessLogicService.GetAuthorizableItemsOfGroup(this.GetUser().Id, groupName));
        }

        /// <summary>Assigns the given group to the given inventory-object, so that its members get read-access to the object and to everything which is in it. Requires administrator privileges.</summary>
        /// <param name="groupName">The name of the group.</param>
        /// <param name="authorizableItemId">The id of the inventory-object.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameAdmins)]
        [HttpPut]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [Route(nameof(AssignGroupToAuthorizableItem))]
        public IActionResult AssignGroupToAuthorizableItem([FromQuery] string groupName, [FromQuery] string authorizableItemId)
        {
            this._BusinessLogicService.AssignGroupToAuthorizableItem(this.GetUser().Id, groupName, authorizableItemId);
            return this.Ok();
        }

        /// <summary>Removes the assignment of the given group to the given inventory-object. Requires administrator privileges.</summary>
        /// <param name="groupName">The name of the group.</param>
        /// <param name="authorizableItemId">The id of the inventory-object.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameAdmins)]
        [HttpPut]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [Route(nameof(RemoveGroupFromAuthorizableItem))]
        public IActionResult RemoveGroupFromAuthorizableItem([FromQuery] string groupName, [FromQuery] string authorizableItemId)
        {
            this._BusinessLogicService.RemoveGroupFromAuthorizableItem(this.GetUser().Id, groupName, authorizableItemId);
            return this.Ok();
        }

        private User GetUser()
        {
            return Tools.GetUser(this.User, this._AuthenticationService);
        }
    }
}
