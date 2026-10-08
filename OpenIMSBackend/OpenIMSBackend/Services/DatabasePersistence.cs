using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.APIServer.Utilities.InitializationStates;
using GRYLibrary.Core.Logging.GRYLogger;
using OpenIMSBackend.Core.Model.BusinessTypes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using GUtilities = GRYLibrary.Core.Misc.Utilities;
using Role = GRYLibrary.Core.APIServer.CommonDBTypes.Role;

namespace OpenIMSBackend.Core.Services
{
    /// <summary>
    /// The persistence which stores everything in a relational database.
    /// <para>
    /// The class itself contains no SQL: every statement is loaded through <see cref="ISQLProvider"/>, which
    /// resolves it to the statement-resource of the database-type in use. That is what makes the support of a
    /// further database-type a matter of adding a folder of statements and of migrations.
    /// </para>
    /// </summary>
    public sealed partial class DatabasePersistence : IInitializable, IPersistence
    {
        private readonly ISQLProvider _SQLProvider;
        private static readonly object _Lock = new object();
        private readonly ITimeService _TimeService;
        private readonly IGRYLog _Log;
        private readonly IOpenIMSDatabaseInteractor _Database;

        public InitializationState InitializationState { get; private set; }

        /// <summary>Initializes a new database-backed persistence instance.</summary>
        /// <param name="database">The database interactor used to execute SQL commands.</param>
        /// <param name="timeService">The service used to obtain the current time.</param>
        /// <param name="log">The logger for diagnostic output.</param>
        public DatabasePersistence(IOpenIMSDatabaseInteractor database, ITimeService timeService, IServerLog log)
        {
            this._TimeService = timeService;
            this._Database = database;
            this._Log = log.Logger;
            this._SQLProvider = database.GetSQLProvider();
            this.InitializationState = new Uninitialized();
        }

        #region AccessDatabase

        private void RunTransaction(string nameOfAction, bool runTransactional, params Action<DbCommand>[] actions)
        {
            lock (_Lock)
            {
                DBUtilities.RunTransaction<IOpenIMSDatabaseInteractor>(nameOfAction, this._Log, this._Database, runTransactional, actions);
            }
        }

        private T?[] RunTransaction<T>(string nameOfAction, bool runTransactional, params Func<DbCommand, T?>[] functions)
        {
            lock (_Lock)
            {
                return DBUtilities.RunTransaction<T, IOpenIMSDatabaseInteractor>(nameOfAction, this._Log, this._Database, runTransactional, functions);
            }
        }

        private DbParameter GetParameter(string name, object? value)
        {
            return this._Database.GetGenericDatabaseInteractor().GetParameter(name, value);
        }

        private DbParameter GetParameter(string name, object? value, Type type)
        {
            return this._Database.GetGenericDatabaseInteractor().GetParameter(name, value, type);
        }

        #endregion

        #region Users

        /// <inheritdoc />
        public IDictionary<string, User> GetAllUsers()
        {
            List<User> users = this.RunTransaction(nameof(GetAllUsers), true, (cmd) =>
            {
                List<User> result = new List<User>();
                cmd.CommandText = this._SQLProvider.GetScriptGetAllUsers();
                using DbDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(this.ReadUser(reader));
                }
                reader.Close();
                return result;
            })[0]!;
            Dictionary<string, User> usersById = new Dictionary<string, User>();
            foreach (User user in users)
            {
                this.EnrichWithRoles(user);
                usersById[user.Id] = user;
            }
            return usersById;
        }

        /// <summary>Reads the columns which every user-query selects (in exactly that order) into a <see cref="User"/>.</summary>
        private User ReadUser(DbDataReader reader)
        {
            User user = new User();
            user.Id = reader.GetString(0);
            user.Name = reader.GetString(1);
            user.PasswordHash = DBUtilities.GetNullableValue<string>(reader, 2);
            user.EMailAddress = DBUtilities.GetNullableValue<string>(reader, 3);
            user.UserIsActivated = reader.GetBoolean(4);
            user.UserIsLocked = reader.GetBoolean(5);
            user.RegistrationMoment = reader.GetDateTime(6);
            return user;
        }

        /// <inheritdoc />
        public void AddUser(User user)
        {
            this.RunTransaction(nameof(AddUser) + "_" + user.Id, true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddUser();
                command.Parameters.Add(this.GetParameter("Id", user.Id));
                command.Parameters.Add(this.GetParameter("Name", user.Name));
                command.Parameters.Add(this.GetParameter("PasswordHash", user.PasswordHash, typeof(string)));
                command.Parameters.Add(this.GetParameter("EMailAddress", user.EMailAddress, typeof(string)));
                command.Parameters.Add(this.GetParameter("UserIsActivated", user.UserIsActivated));
                command.Parameters.Add(this.GetParameter("UserIsLocked", user.UserIsLocked));
                command.Parameters.Add(this.GetParameter("RegistrationMoment", user.RegistrationMoment));
                command.Parameters.Add(this.GetParameter("TOTPActivated", user.TOTP?.IsActicated, typeof(bool)));
                command.Parameters.Add(this.GetParameter("TOTPSecretKey", user.TOTP?.SecretKey, typeof(string)));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public void UpdateUser(User user)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void RemoveUser(string userId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool UserWithNameExists(string userName)
        {
            return this.RunTransaction(nameof(UserWithNameExists), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUserWithNameExists();
                cmd.Parameters.Add(this.GetParameter("UserName", userName));
                using DbDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        /// <inheritdoc />
        public bool UserWithIdExists(string userId)
        {
            return this.RunTransaction(nameof(UserWithIdExists), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUserWithIdExists();
                cmd.Parameters.Add(this.GetParameter("UserId", userId));
                using DbDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        /// <inheritdoc />
        public User GetUserById(string userId)
        {
            User result = GUtilities.GetValue(this.RunTransaction(nameof(GetUserById), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetUserById();
                cmd.Parameters.Add(this.GetParameter("Id", userId));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    return this.ReadUser(reader);
                }
                else
                {
                    throw new KeyNotFoundException($"No user found with id '{userId}'");
                }
            })[0]);
            this.EnrichWithRoles(result);
            this.EnrichWithAccessToken(result);
            return result;
        }

        /// <inheritdoc />
        public User GetUserByName(string userName)
        {
            User result = GUtilities.GetValue(this.RunTransaction(nameof(GetUserByName) + "_" + userName, true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetUserByName();
                cmd.Parameters.Add(this.GetParameter("Name", userName));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    return this.ReadUser(reader);
                }
                else
                {
                    throw new KeyNotFoundException($"No user found with username '{userName}'");
                }
            })[0]);
            this.EnrichWithRoles(result);
            this.EnrichWithAccessToken(result);
            return result;
        }

        /// <inheritdoc />
        public uint GetAmountOfUsers()
        {
            return this.RunTransaction(nameof(GetAmountOfUsers), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetAmountOfUsers();
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    //"count(*)" is a 64-bit value in both supported databases, so it must not be read as a 32-bit one.
                    return (uint)reader.GetFieldValue<long>(0);
                }
                else
                {
                    return (uint)0;
                }
            })[0];
        }

        private void EnrichWithRoles(User user)
        {
            foreach (Role role in this.GetAllRoles())
            {
                if (this.UserHasRole(user.Id, role.Id))
                {
                    user.Roles.Add(role);
                }
            }
        }

        private void EnrichWithAccessToken(User user)
        {
            this.RunTransaction(nameof(EnrichWithAccessToken), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetAllAccessTokenForUser();
                cmd.Parameters.Add(this.GetParameter("UserId", user.Id));
                using DbDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    user.AccessToken.Add(new AccessToken()
                    {
                        Value = reader.GetString(0),
                        ExpiredMoment = reader.GetDateTime(1),
                        OwnerUserId = user.Id
                    });
                }
            });
        }

        #endregion

        #region Roles

        /// <inheritdoc />
        public ISet<Role> GetAllRoles()
        {
            ISet<Role> roles = this.RunTransaction(nameof(GetAllRoles), true, (command) =>
            {
                ISet<Role> rolesInternal = new HashSet<Role>();
                command.CommandText = this._SQLProvider.GetScriptGetAllRoles();
                using DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    rolesInternal.Add(new Role() { Id = reader.GetString(0), Name = reader.GetString(1) });
                }
                reader.Close();
                return rolesInternal;
            })[0]!;
            foreach (Role role in roles)
            {
                this.EnrichWithDirectlyInheritedRoles(role);
            }
            return roles;
        }

        private void EnrichWithDirectlyInheritedRoles(Role role)
        {
            ISet<string> inheritedRoleIds = this.RunTransaction(nameof(EnrichWithDirectlyInheritedRoles), true, (command) =>
            {
                ISet<string> inheritedRoleIdsInternal = new HashSet<string>();
                command.CommandText = this._SQLProvider.GetScriptGetDirectlyInheritedRoleIds();
                command.Parameters.Add(this.GetParameter("RoleId", role.Id));
                using DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    inheritedRoleIdsInternal.Add(reader.GetString(0));
                }
                reader.Close();
                return inheritedRoleIdsInternal;
            })[0]!;
            foreach (string inheritedRoleId in inheritedRoleIds)
            {
                role.DirectlyInheritedRoles.Add(this.GetRoleById(inheritedRoleId));
            }
        }

        /// <inheritdoc />
        public void AddRole(Role role)
        {
            this.RunTransaction(nameof(AddRole), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptInsertRole();
                command.Parameters.Add(this.GetParameter("Id", role.Id));
                command.Parameters.Add(this.GetParameter("Name", role.Name));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public void UpdateRole(Role role)
        {
            this.RunTransaction(nameof(UpdateRole), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUpdateRole();
                cmd.Parameters.Add(this.GetParameter("Id", role.Id));
                cmd.Parameters.Add(this.GetParameter("Name", role.Name));
                cmd.ExecuteNonQuery();
            }, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptDeleteDirectlyInheritedRoles();
                cmd.Parameters.Add(this.GetParameter("RoleId", role.Id));
                cmd.ExecuteNonQuery();
            }, (cmd) =>
            {
                if (role.DirectlyInheritedRoles.Any())
                {
                    List<string> insertLines = new List<string>();
                    uint index = 0;
                    cmd.Parameters.Clear();
                    foreach (Role directlyInheritedRole in role.DirectlyInheritedRoles)
                    {
                        insertLines.Add($"(@RoleId{index}, @InheritedRoleId{index})");//attention: this syntax must always be both: mariadb-compliant and postgresql-compliant
                        cmd.Parameters.Add(this.GetParameter($"RoleId{index}", role.Id));
                        cmd.Parameters.Add(this.GetParameter($"InheritedRoleId{index}", directlyInheritedRole.Id));
                        index++;
                    }
                    cmd.CommandText = this._SQLProvider.GetScriptAddDirectlyInheritedRoles().Replace("__generated__", string.Join(",\n        ", insertLines));
                    cmd.ExecuteNonQuery();
                }
            });
        }

        /// <inheritdoc />
        public void DeleteRoleByName(string roleName)
        {
            Role role = this.GetRoleByName(roleName);
            this.RunTransaction(nameof(DeleteRoleByName) + "_" + role.Id, true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptDeleteRole();
                command.Parameters.Add(this.GetParameter("RoleId", role.Id));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public bool RoleExists(string roleName)
        {
            return this.RunTransaction(nameof(RoleExists), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptRoleExists();
                cmd.Parameters.Add(this.GetParameter("RoleName", roleName));
                using DbDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        /// <inheritdoc />
        public Role GetRoleByName(string roleName)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetRoleByName), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetRoleByName();
                cmd.Parameters.Add(this.GetParameter("Name", roleName));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    return new Role() { Id = reader.GetString(0), Name = roleName };
                }
                else
                {
                    throw new KeyNotFoundException($"No role found with name '{roleName}'");
                }
            })[0]);
        }

        /// <inheritdoc />
        public Role GetRoleById(string roleId)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetRoleById), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetRoleById();
                cmd.Parameters.Add(this.GetParameter("Id", roleId));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    return new Role() { Id = roleId, Name = reader.GetString(0) };
                }
                else
                {
                    throw new KeyNotFoundException($"No role found with id '{roleId}'");
                }
            })[0]);
        }

        /// <inheritdoc />
        public void AddRoleToUser(string userId, string roleId)
        {
            this.RunTransaction(nameof(AddRoleToUser), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddRoleToUser();
                command.Parameters.Add(this.GetParameter("UserId", userId));
                command.Parameters.Add(this.GetParameter("RoleId", roleId));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public void RemoveRoleFromUser(string userId, string roleId)
        {
            this.RunTransaction(nameof(RemoveRoleFromUser), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptRemoveRoleFromUser();
                command.Parameters.Add(this.GetParameter("UserId", userId));
                command.Parameters.Add(this.GetParameter("RoleId", roleId));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public bool UserHasRole(string userId, string roleId)
        {
            return this.RunTransaction(nameof(UserHasRole), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUserHasRole();
                cmd.Parameters.Add(this.GetParameter("UserId", userId));
                cmd.Parameters.Add(this.GetParameter("RoleId", roleId));
                using DbDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        #endregion

        #region AccessToken

        /// <inheritdoc />
        public bool AccessTokenExists(string accessToken, out User user)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public AccessToken GetAccessToken(string accessToken)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetAccessToken), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetAccessToken();
                cmd.Parameters.Add(this.GetParameter("Value", accessToken));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    AccessToken result = new AccessToken();
                    result.Value = accessToken;
                    result.ExpiredMoment = new DateTimeOffset(reader.GetFieldValue<DateTime>(0));
                    result.OwnerUserId = reader.GetString(1);
                    return result;
                }
                else
                {
                    throw new KeyNotFoundException($"No access-token found with value '{accessToken}'");
                }
            })[0]);
        }

        /// <inheritdoc />
        public void AddAccessToken(AccessToken newAccessToken)
        {
            this.RunTransaction(nameof(AddAccessToken), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptAddAccessToken();
                cmd.Parameters.Add(this.GetParameter("Value", newAccessToken.Value));
                cmd.Parameters.Add(this.GetParameter("ExpiredMoment", newAccessToken.ExpiredMoment));
                cmd.Parameters.Add(this.GetParameter("UserId", newAccessToken.OwnerUserId));
                cmd.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public void RemoveAccessToken(string accessToken)
        {
            this.RunTransaction(nameof(RemoveAccessToken), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptRemoveAccessToken();
                cmd.Parameters.Add(this.GetParameter("Value", accessToken));
                cmd.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public ISet<AccessToken> GetAllAccessTokenOfUser(string userId)
        {
            return this.RunTransaction(nameof(GetAllAccessTokenOfUser), true, (cmd) =>
            {
                ISet<AccessToken> result = new HashSet<AccessToken>();
                cmd.CommandText = this._SQLProvider.GetScriptGetAllAccessTokenForUser();
                cmd.Parameters.Add(this.GetParameter("UserId", userId));
                using DbDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new AccessToken()
                    {
                        Value = reader.GetString(0),
                        ExpiredMoment = reader.GetDateTime(1),
                        OwnerUserId = userId
                    });
                }
                reader.Close();
                return result;
            })[0]!;
        }

        /// <inheritdoc />
        public User GetUserByAccessToken(string accessToken)
        {
            return this.GetUserById(this.GetAccessToken(accessToken).OwnerUserId);
        }

        #endregion

        #region Settings

        /// <inheritdoc />
        public string? GetSetting(string key)
        {
            return this.RunTransaction(nameof(GetSetting), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptGetSetting();
                command.Parameters.Add(this.GetParameter("Key", key));
                using DbDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    return reader.GetString(0);
                }
                else
                {
                    return null;
                }
            })[0];
        }

        /// <inheritdoc />
        public void SetSetting(string key, string value)
        {
            this.RunTransaction(nameof(SetSetting), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptSetSetting();
                command.Parameters.Add(this.GetParameter("Key", key));
                command.Parameters.Add(this.GetParameter("Value", value));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public string? GetUserSetting(string userId, string key)
        {
            return this.RunTransaction(nameof(GetUserSetting), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptGetUserSetting();
                command.Parameters.Add(this.GetParameter("UserId", userId));
                command.Parameters.Add(this.GetParameter("Key", key));
                using DbDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    return reader.GetString(0);
                }
                else
                {
                    return null;
                }
            })[0];
        }

        /// <inheritdoc />
        public void SetUserSetting(string userId, string key, string value)
        {
            this.RunTransaction(nameof(SetUserSetting), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptSetUserSetting();
                command.Parameters.Add(this.GetParameter("UserId", userId));
                command.Parameters.Add(this.GetParameter("Key", key));
                command.Parameters.Add(this.GetParameter("Value", value));
                command.ExecuteNonQuery();
            });
        }

        #endregion

        /// <inheritdoc />
        public (bool, Exception?) IsAvailable()
        {
            lock (_Lock)
            {
                bool result = this._Database.GetGenericDatabaseInteractor().TryGetConnection(out _, out Exception? e);
                return (result, e);
            }
        }

        /// <inheritdoc />
        public void SetLogConnectionAttemptErrors(bool enabled)
        {
            this._Database.SetLogConnectionAttemptErrors(enabled);
        }

        /// <inheritdoc />
        public void WaitUntilAvailable(TimeSpan timeSpan)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Initialize()
        {
            lock (_Lock)
            {
                try
                {
                    this.InitializationState = new Initializing();
                    this._Database.GetGenericDatabaseInteractor().DoAllMigrations(this._Database.GetAllMigrations(), this._TimeService);
                    this.InitializationState = new Initialized();
                }
                catch (Exception ex)
                {
                    this.InitializationState = new InitializationFailed();
                    this._Log.Log("Initialization failed", ex);
                }
            }
        }

        /// <inheritdoc />
        public void Reset()
        {
            this.RunTransaction(nameof(Reset), false, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptResetDatabase();
                cmd.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            //the connection is owned by the database-interactor, which is disposed by whoever created it.
            GUtilities.NoOperation();
        }
    }
}
