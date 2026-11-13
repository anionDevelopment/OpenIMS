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

        /// <summary>Returns the complete inventory as a tree. Every logged-in user may do this; the result is not restricted by groups.</summary>
        /// <returns>An array of <see cref="InventoryTreeNodeDTO"/>, one per location, ordered by the name.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpGet]
        [ProducesResponseType(typeof(InventoryTreeNodeDTO[]), StatusCodes.Status200OK)]
        [Route(nameof(GetInventoryTree))]
        public IActionResult GetInventoryTree()
        {
            return this.Ok(this._BusinessLogicService.GetInventoryTree());
        }

        /// <summary>Creates a new location. Every logged-in user may do this.</summary>
        /// <param name="data">Name and description of the new location.</param>
        /// <returns>A <see cref="StringValueDTO"/> with the id of the created location.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpPost]
        [ProducesResponseType(typeof(StringValueDTO), StatusCodes.Status200OK)]
        [Route(nameof(CreateLocation))]
        public IActionResult CreateLocation([FromBody] InventoryObjectDataDTO data)
        {
            return this.OkWithId(this._BusinessLogicService.CreateLocation(data.Name, data.Description));
        }

        /// <summary>Creates a new building in the given location. Every logged-in user may do this.</summary>
        /// <param name="parentId">The id of the location.</param>
        /// <param name="data">Name and description of the new building.</param>
        /// <returns>A <see cref="StringValueDTO"/> with the id of the created building.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpPost]
        [ProducesResponseType(typeof(StringValueDTO), StatusCodes.Status200OK)]
        [Route(nameof(CreateBuilding))]
        public IActionResult CreateBuilding([FromQuery] string parentId, [FromBody] InventoryObjectDataDTO data)
        {
            return this.OkWithId(this._BusinessLogicService.CreateBuilding(parentId, data.Name, data.Description));
        }

        /// <summary>Creates a new area (for example a room or a yard) in the given location or building. Every logged-in user may do this.</summary>
        /// <param name="parentId">The id of the location or building.</param>
        /// <param name="data">Name and description of the new area.</param>
        /// <returns>A <see cref="StringValueDTO"/> with the id of the created area.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpPost]
        [ProducesResponseType(typeof(StringValueDTO), StatusCodes.Status200OK)]
        [Route(nameof(CreateArea))]
        public IActionResult CreateArea([FromQuery] string parentId, [FromBody] InventoryObjectDataDTO data)
        {
            return this.OkWithId(this._BusinessLogicService.CreateArea(parentId, data.Name, data.Description));
        }

        /// <summary>Creates a new container (for example a cupboard or a box) in the given area or container. Every logged-in user may do this.</summary>
        /// <param name="parentId">The id of the area or container.</param>
        /// <param name="data">Name and description of the new container.</param>
        /// <returns>A <see cref="StringValueDTO"/> with the id of the created container.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpPost]
        [ProducesResponseType(typeof(StringValueDTO), StatusCodes.Status200OK)]
        [Route(nameof(CreateContainer))]
        public IActionResult CreateContainer([FromQuery] string parentId, [FromBody] InventoryObjectDataDTO data)
        {
            return this.OkWithId(this._BusinessLogicService.CreateContainer(parentId, data.Name, data.Description));
        }

        /// <summary>Creates a new item in the given area or container. The area or container is the target place and the real place of the new item. Every logged-in user may do this.</summary>
        /// <param name="parentId">The id of the area or container.</param>
        /// <param name="data">Name and description of the new item.</param>
        /// <returns>A <see cref="StringValueDTO"/> with the id of the created item.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpPost]
        [ProducesResponseType(typeof(StringValueDTO), StatusCodes.Status200OK)]
        [Route(nameof(CreateItem))]
        public IActionResult CreateItem([FromQuery] string parentId, [FromBody] InventoryObjectDataDTO data)
        {
            return this.OkWithId(this._BusinessLogicService.CreateItem(parentId, parentId, data.Name, data.Description));
        }

        /// <summary>Changes name and description of the given inventory-object. Every logged-in user may do this.</summary>
        /// <param name="inventoryObjectId">The id of the inventory-object.</param>
        /// <param name="data">The new name and description.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpPut]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [Route(nameof(UpdateInventoryObject))]
        public IActionResult UpdateInventoryObject([FromQuery] string inventoryObjectId, [FromBody] InventoryObjectDataDTO data)
        {
            this._BusinessLogicService.UpdateInventoryObject(inventoryObjectId, data.Name, data.Description);
            return this.Ok();
        }

        /// <summary>Deletes the given inventory-object. Only an object which contains nothing and where no item actually is can be deleted. Every logged-in user may do this.</summary>
        /// <param name="inventoryObjectId">The id of the inventory-object.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpDelete]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [Route(nameof(DeleteInventoryObject))]
        public IActionResult DeleteInventoryObject([FromQuery] string inventoryObjectId)
        {
            this._BusinessLogicService.DeleteInventoryObject(inventoryObjectId);
            return this.Ok();
        }

        private IActionResult OkWithId(string id)
        {
            return this.Ok(new StringValueDTO() { Value = id });
        }

        private User GetUser()
        {
            return Tools.GetUser(this.User, this._AuthenticationService);
        }
    }
}
