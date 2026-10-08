namespace OpenIMSBackend.Core.Services
{
    /// <summary>Provides every SQL-statement which the backend executes. One implementation exists per supported database-type, so that a statement which has to be written differently for another database is a difference between two resource-folders and not a difference in the business-logic.</summary>
    public interface ISQLProvider
    {
        string GetScriptResetDatabase();

        #region Users
        string GetScriptAddUser();
        string GetScriptGetAllUsers();
        string GetScriptGetUserById();
        string GetScriptGetUserByName();
        string GetScriptUserWithIdExists();
        string GetScriptUserWithNameExists();
        string GetScriptGetAmountOfUsers();
        #endregion

        #region Roles
        string GetScriptInsertRole();
        string GetScriptUpdateRole();
        string GetScriptRoleExists();
        string GetScriptGetAllRoles();
        string GetScriptGetRoleById();
        string GetScriptGetRoleByName();
        string GetScriptAddRoleToUser();
        string GetScriptRemoveRoleFromUser();
        string GetScriptUserHasRole();
        string GetScriptGetRolesOfUser();
        string GetScriptGetDirectlyInheritedRoleIds();
        string GetScriptDeleteDirectlyInheritedRoles();
        string GetScriptAddDirectlyInheritedRoles();
        string GetScriptDeleteRole();
        #endregion

        #region AccessToken
        string GetScriptAddAccessToken();
        string GetScriptGetAccessToken();
        string GetScriptRemoveAccessToken();
        string GetScriptGetAllAccessTokenForUser();
        #endregion

        #region Settings
        string GetScriptGetSetting();
        string GetScriptSetSetting();
        string GetScriptGetUserSetting();
        string GetScriptSetUserSetting();
        #endregion

        #region Inventory
        string GetScriptAddInventoryObject();
        string GetScriptUpdateInventoryObject();
        string GetScriptRemoveInventoryObject();
        string GetScriptInventoryObjectExists();
        string GetScriptGetInventoryObject();
        string GetScriptGetAllInventoryObjects();
        string GetScriptInventoryObjectHasChildren();
        string GetScriptInventoryObjectIsUsedAsRealPlace();
        string GetScriptSearchInventoryObjects();
        #endregion

        #region Authorization of authorizable items
        string GetScriptAddRoleToAuthorizableItem();
        string GetScriptRemoveRoleFromAuthorizableItem();
        string GetScriptRemoveAllRolesFromAuthorizableItem();
        string GetScriptGetRoleIdsOfAuthorizableItem();
        string GetScriptGetAuthorizableItemIdsOfRole();
        #endregion
    }
}
