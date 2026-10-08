using GRYLibrary.Core.APIServer.Services.Database;
using OpenIMSBackend.Core.Model.PersistenceTypes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace OpenIMSBackend.Core.Services
{
    /// <summary>The part of <see cref="DatabasePersistence"/> which stores the inventory (table <c>InventoryObjects</c>).</summary>
    public sealed partial class DatabasePersistence
    {
        /// <inheritdoc />
        public void AddInventoryObject(InventoryObjectRecord inventoryObject)
        {
            this.RunTransaction(nameof(AddInventoryObject) + "_" + inventoryObject.Id, true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddInventoryObject();
                this.AddCommonInventoryObjectParameters(command, inventoryObject);
                command.Parameters.Add(this.GetParameter("Type", inventoryObject.Type.ToString()));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public void UpdateInventoryObject(InventoryObjectRecord inventoryObject)
        {
            this.RunTransaction(nameof(UpdateInventoryObject) + "_" + inventoryObject.Id, true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptUpdateInventoryObject();
                this.AddCommonInventoryObjectParameters(command, inventoryObject);
                command.ExecuteNonQuery();
            });
        }

        /// <summary>Adds the parameters which the insert- and the update-statement have in common.</summary>
        private void AddCommonInventoryObjectParameters(DbCommand command, InventoryObjectRecord inventoryObject)
        {
            command.Parameters.Add(this.GetParameter("Id", inventoryObject.Id));
            command.Parameters.Add(this.GetParameter("Name", inventoryObject.Name));
            command.Parameters.Add(this.GetParameter("Description", inventoryObject.Description));
            command.Parameters.Add(this.GetParameter("ParentId", inventoryObject.ParentId, typeof(string)));
            command.Parameters.Add(this.GetParameter("RealPlaceId", inventoryObject.RealPlaceId, typeof(string)));
        }

        /// <inheritdoc />
        public void RemoveInventoryObject(string inventoryObjectId)
        {
            // Both statements run in one transaction, so that an object never loses its assigned roles without being removed.
            this.RunTransaction(nameof(RemoveInventoryObject) + "_" + inventoryObjectId, true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptRemoveAllRolesFromAuthorizableItem();
                command.Parameters.Add(this.GetParameter("AuthorizableItemId", inventoryObjectId));
                command.ExecuteNonQuery();
            }, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptRemoveInventoryObject();
                command.Parameters.Add(this.GetParameter("Id", inventoryObjectId));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public bool InventoryObjectExists(string inventoryObjectId)
        {
            return this.QueryHasRows(nameof(InventoryObjectExists), this._SQLProvider.GetScriptInventoryObjectExists(), inventoryObjectId);
        }

        /// <inheritdoc />
        public InventoryObjectRecord GetInventoryObject(string inventoryObjectId)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetInventoryObject), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptGetInventoryObject();
                command.Parameters.Add(this.GetParameter("Id", inventoryObjectId));
                using DbDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return ReadInventoryObject(reader);
                }
                else
                {
                    throw new KeyNotFoundException($"No inventory-object found with id '{inventoryObjectId}'");
                }
            })[0]);
        }

        /// <inheritdoc />
        public IList<InventoryObjectRecord> GetAllInventoryObjects()
        {
            return this.RunTransaction(nameof(GetAllInventoryObjects), true, (command) =>
            {
                List<InventoryObjectRecord> result = new List<InventoryObjectRecord>();
                command.CommandText = this._SQLProvider.GetScriptGetAllInventoryObjects();
                using DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(ReadInventoryObject(reader));
                }
                return result;
            })[0]!;
        }

        /// <inheritdoc />
        public bool InventoryObjectHasChildren(string inventoryObjectId)
        {
            return this.QueryHasRows(nameof(InventoryObjectHasChildren), this._SQLProvider.GetScriptInventoryObjectHasChildren(), inventoryObjectId);
        }

        /// <inheritdoc />
        public bool InventoryObjectIsUsedAsRealPlace(string inventoryObjectId)
        {
            return this.QueryHasRows(nameof(InventoryObjectIsUsedAsRealPlace), this._SQLProvider.GetScriptInventoryObjectIsUsedAsRealPlace(), inventoryObjectId);
        }

        /// <inheritdoc />
        public IList<InventoryObjectRecord> SearchInventoryObjects(string searchText, ISet<InventoryObjectType> types, int maximalAmountOfResults)
        {
            return this.RunTransaction(nameof(SearchInventoryObjects), true, (command) =>
            {
                List<InventoryObjectRecord> result = new List<InventoryObjectRecord>();
                command.CommandText = this._SQLProvider.GetScriptSearchInventoryObjects();
                command.Parameters.Add(this.GetParameter("SearchPattern", "%" + EscapeForLike(searchText) + "%"));
                command.Parameters.Add(this.GetParameter("IncludeLocations", types.Contains(InventoryObjectType.Location)));
                command.Parameters.Add(this.GetParameter("IncludeBuildings", types.Contains(InventoryObjectType.Building)));
                command.Parameters.Add(this.GetParameter("IncludeAreas", types.Contains(InventoryObjectType.Area)));
                command.Parameters.Add(this.GetParameter("IncludeContainers", types.Contains(InventoryObjectType.Container)));
                command.Parameters.Add(this.GetParameter("IncludeItems", types.Contains(InventoryObjectType.Item)));
                command.Parameters.Add(this.GetParameter("MaximalAmountOfResults", maximalAmountOfResults));
                using DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(ReadInventoryObject(reader));
                }
                return result;
            })[0]!;
        }

        /// <summary>Escapes the characters which have a special meaning in a like-pattern, so that the text is matched literally. The backslash is the default escape-character of like in MariaDB and PostgreSQL.</summary>
        private static string EscapeForLike(string text)
        {
            return text.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
        }

        /// <summary>Executes a query which has the id of an inventory-object as only parameter and returns whether it returned at least one row.</summary>
        private bool QueryHasRows(string nameOfAction, string query, string inventoryObjectId)
        {
            return this.RunTransaction(nameOfAction, true, (command) =>
            {
                command.CommandText = query;
                command.Parameters.Add(this.GetParameter("Id", inventoryObjectId));
                using DbDataReader reader = command.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        /// <summary>Reads the columns which every inventory-object-query selects (in exactly that order) into an <see cref="InventoryObjectRecord"/>.</summary>
        private static InventoryObjectRecord ReadInventoryObject(DbDataReader reader)
        {
            return new InventoryObjectRecord(
                reader.GetString(0),
                Enum.Parse<InventoryObjectType>(reader.GetString(1)),
                reader.GetString(2),
                reader.GetString(3),
                DBUtilities.GetNullableValue<string>(reader, 4),
                DBUtilities.GetNullableValue<string>(reader, 5));
        }
    }
}
