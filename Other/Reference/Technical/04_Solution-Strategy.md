# 4. Solution Strategy

## Authentication

A user logs in with a username and a password.
The password itself is never stored: only its hash is persisted in the column `PasswordHash` of the table `Users`.

A successful login creates an access-token which is stored in the table `AccessToken` together with the moment it expires and the user it belongs to.
Every authenticated request carries that token in the header `X-AccessToken`; the authentication-middleware resolves the user from it.
A logout removes the token, so it can not be used anymore.

The routes which are reachable without a valid access-token are listed explicitly in `Program.cs` (`RoutesWhereUnauthenticatedAccessIsAllowed`).
Everything which is not listed there requires authentication.

## Authorization

Authorization is role-based.
Every user has at least the role `Users`; an administrator additionally has the role `Administrators`, which inherits `Users`.
A role can inherit other roles (table `Role_InheritedRoles`), so a permission which is granted to an inherited role applies to the inheriting role as well.

The role which a route requires is declared at the route itself (attribute `Authorize`), so the requirement of an endpoint is visible where the endpoint is defined.

## Registration

A user-account is created by an administrator (`UserController.Register`).
Whether an account can be created at all is decided by the configuration-value `RegistrationIsEnabled`.

On the very first start the initialization-service creates the roles `Users` and `Administrators` and the user `admin` with the password which was passed as `InitialAdminPassword`.
That step is not idempotent and is therefore only executed when the admin-user does not exist yet.

## Persistence

The business-data is stored in a relational database.
Which database is used is decided by the configuration-value `DatabaseType`; see [the article about the supported databases](../../../OpenIMSBackend/Other/Reference/ReferenceContent/Articles/SupportedDatabases.md).

The schema is not created by an object-relational mapper but by explicit migration-scripts (`OpenIMSBackend/OpenIMSBackend/Resources/Database/<database-type>/Migrations`), one script per migration and one set of scripts per supported database-type.
Running all migrations is idempotent and is done on every start, so an update of the container applies the migrations of the new version automatically.

Every SQL-statement which the backend executes is an own embedded resource (`Resources/Database/<database-type>/Statements`) and is loaded through the `ISQLProvider`-abstraction.
This keeps the statements out of the C#-code and makes the difference between two database-types a difference between two resource-folders instead of a difference in the business-logic.

## Example-data

In the environment `Development` the initialization creates example-data (`ExampleDataCreator`), so that a freshly started backend is usable immediately without setting up anything by hand.
The example-data consists of user-accounts only; it does not contain any business-content.
