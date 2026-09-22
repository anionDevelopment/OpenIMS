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
    }
}
