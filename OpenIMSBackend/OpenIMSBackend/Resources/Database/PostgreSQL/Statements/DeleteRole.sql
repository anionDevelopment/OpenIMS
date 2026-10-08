
-- Removes everything which references the role first, because the foreign-keys do not allow to delete a referenced role.
-- A role which is inherited by another role must not be deleted; the backend ensures this before it executes this statement.
delete from "Role_AuthorizableItems" where "RoleId"=@RoleId;
delete from "User_Roles" where "RoleId"=@RoleId;
delete from "Role_InheritedRoles" where "RoleId"=@RoleId;
delete from "Roles" where "Id"=@RoleId;
