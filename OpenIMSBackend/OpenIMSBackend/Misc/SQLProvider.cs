using GRYLibrary.Core.Misc;
using OpenIMSBackend.Core.Services;

namespace OpenIMSBackend.Core.Misc
{
    public abstract class SQLProvider : AbstractSQLProvider, ISQLProvider
    {
        /// <summary>Initializes the SQL provider for the specified database type.</summary>
        /// <param name="databaseType">The database type identifier used to locate the embedded SQL script folder (e.g. "MariaDB" or "PostgreSQL").</param>
        public SQLProvider(string databaseType) : base($"OpenIMSBackend.Core.Resources.Database.{databaseType}.Statements") { }

        /// <inheritdoc />
        public string GetScriptResetDatabase()
        {
            return this.LoadSQLScript("ResetDatabase");
        }

        #region Users

        /// <inheritdoc />
        public string GetScriptAddUser()
        {
            return this.LoadSQLScript("AddUser");
        }

        /// <inheritdoc />
        public string GetScriptGetAllUsers()
        {
            return this.LoadSQLScript("GetAllUsers");
        }

        /// <inheritdoc />
        public string GetScriptGetUserById()
        {
            return this.LoadSQLScript("GetUserById");
        }

        /// <inheritdoc />
        public string GetScriptGetUserByName()
        {
            return this.LoadSQLScript("GetUserByName");
        }

        /// <inheritdoc />
        public string GetScriptUserWithIdExists()
        {
            return this.LoadSQLScript("UserWithIdExists");
        }

        /// <inheritdoc />
        public string GetScriptUserWithNameExists()
        {
            return this.LoadSQLScript("UserWithNameExists");
        }

        /// <inheritdoc />
        public string GetScriptGetAmountOfUsers()
        {
            return this.LoadSQLScript("GetAmountOfUsers");
        }

        #endregion

        #region Roles

        /// <inheritdoc />
        public string GetScriptInsertRole()
        {
            return this.LoadSQLScript("InsertRole");
        }

        /// <inheritdoc />
        public string GetScriptUpdateRole()
        {
            return this.LoadSQLScript("UpdateRole");
        }

        /// <inheritdoc />
        public string GetScriptRoleExists()
        {
            return this.LoadSQLScript("RoleExists");
        }

        /// <inheritdoc />
        public string GetScriptGetAllRoles()
        {
            return this.LoadSQLScript("GetAllRoles");
        }

        /// <inheritdoc />
        public string GetScriptGetRoleById()
        {
            return this.LoadSQLScript("GetRoleById");
        }

        /// <inheritdoc />
        public string GetScriptGetRoleByName()
        {
            return this.LoadSQLScript("GetRoleByName");
        }

        /// <inheritdoc />
        public string GetScriptAddRoleToUser()
        {
            return this.LoadSQLScript("AddRoleToUser");
        }

        /// <inheritdoc />
        public string GetScriptRemoveRoleFromUser()
        {
            return this.LoadSQLScript("RemoveRoleFromUser");
        }

        /// <inheritdoc />
        public string GetScriptUserHasRole()
        {
            return this.LoadSQLScript("UserHasRole");
        }

        /// <inheritdoc />
        public string GetScriptGetRolesOfUser()
        {
            return this.LoadSQLScript("GetRolesOfUser");
        }

        /// <inheritdoc />
        public string GetScriptGetDirectlyInheritedRoleIds()
        {
            return this.LoadSQLScript("GetDirectlyInheritedRoleIds");
        }

        /// <inheritdoc />
        public string GetScriptDeleteDirectlyInheritedRoles()
        {
            return this.LoadSQLScript("DeleteDirectlyInheritedRoles");
        }

        /// <inheritdoc />
        public string GetScriptAddDirectlyInheritedRoles()
        {
            return this.LoadSQLScript("AddDirectlyInheritedRoles");
        }

        #endregion

        #region AccessToken

        /// <inheritdoc />
        public string GetScriptAddAccessToken()
        {
            return this.LoadSQLScript("AddAccessToken");
        }

        /// <inheritdoc />
        public string GetScriptGetAccessToken()
        {
            return this.LoadSQLScript("GetAccessToken");
        }

        /// <inheritdoc />
        public string GetScriptRemoveAccessToken()
        {
            return this.LoadSQLScript("RemoveAccessToken");
        }

        /// <inheritdoc />
        public string GetScriptGetAllAccessTokenForUser()
        {
            return this.LoadSQLScript("GetAllAccessTokenForUser");
        }

        #endregion

        #region Settings

        /// <inheritdoc />
        public string GetScriptGetSetting()
        {
            return this.LoadSQLScript("GetSetting");
        }

        /// <inheritdoc />
        public string GetScriptSetSetting()
        {
            return this.LoadSQLScript("SetSetting");
        }

        /// <inheritdoc />
        public string GetScriptGetUserSetting()
        {
            return this.LoadSQLScript("GetUserSetting");
        }

        /// <inheritdoc />
        public string GetScriptSetUserSetting()
        {
            return this.LoadSQLScript("SetUserSetting");
        }

        #endregion
    }
}
