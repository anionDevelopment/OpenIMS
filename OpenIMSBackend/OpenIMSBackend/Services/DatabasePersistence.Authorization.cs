using System.Collections.Generic;
using System.Data.Common;

namespace OpenIMSBackend.Core.Services
{
    /// <summary>The part of <see cref="DatabasePersistence"/> which stores which roles (groups) are assigned to which authorizable items (table <c>Role_AuthorizableItems</c>).</summary>
    public sealed partial class DatabasePersistence
    {
        /// <inheritdoc />
        public void AddRoleToAuthorizableItem(string roleId, string authorizableItemId)
        {
            this.RunTransaction(nameof(AddRoleToAuthorizableItem), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddRoleToAuthorizableItem();
                command.Parameters.Add(this.GetParameter("RoleId", roleId));
                command.Parameters.Add(this.GetParameter("AuthorizableItemId", authorizableItemId));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public void RemoveRoleFromAuthorizableItem(string roleId, string authorizableItemId)
        {
            this.RunTransaction(nameof(RemoveRoleFromAuthorizableItem), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptRemoveRoleFromAuthorizableItem();
                command.Parameters.Add(this.GetParameter("RoleId", roleId));
                command.Parameters.Add(this.GetParameter("AuthorizableItemId", authorizableItemId));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public ISet<string> GetRoleIdsOfAuthorizableItem(string authorizableItemId)
        {
            return this.RunTransaction(nameof(GetRoleIdsOfAuthorizableItem), true, (command) =>
            {
                ISet<string> result = new HashSet<string>();
                command.CommandText = this._SQLProvider.GetScriptGetRoleIdsOfAuthorizableItem();
                command.Parameters.Add(this.GetParameter("AuthorizableItemId", authorizableItemId));
                using DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(reader.GetString(0));
                }
                return result;
            })[0]!;
        }

        /// <inheritdoc />
        public ISet<string> GetAuthorizableItemIdsOfRole(string roleId)
        {
            return this.RunTransaction(nameof(GetAuthorizableItemIdsOfRole), true, (command) =>
            {
                ISet<string> result = new HashSet<string>();
                command.CommandText = this._SQLProvider.GetScriptGetAuthorizableItemIdsOfRole();
                command.Parameters.Add(this.GetParameter("RoleId", roleId));
                using DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(reader.GetString(0));
                }
                return result;
            })[0]!;
        }
    }
}
