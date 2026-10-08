# Hints

## Requirements

The following tools from the [tools-list](https://github.com/anionDev/ScriptCollection/blob/main/ScriptCollection/Other/Reference/ReferenceContent/Articles/RequirementsForCommonProjectStructure.md#Tools) are required to build this code-unit:

- `docfx`
- `docker`
- `dotnet-coverage`
- `git`
- `gitversion`
- `java`
- `python`
- `reportgenerator`
- `scriptcollection`
- `swagger`

## IDE

The recommended IDE for this codeunit is [Visual Studio](https://visualstudio.com/).

Start the project using the usual debug-button in Visual Studio.
When you start the backend the first time then a configuration-file will be generated which will be used.
This configuration-file is located in `<repository-root>\OpenIMSBackend\Other\Workspace\Configuration\Configuration.xml`.
This configuration-file will always be used if available when running the backend locally.
If a debugger is attached (which is typically the case when developing and starting the backend in the IDE) when the configuration-file is generated then an internal transient database-storage will be used instead of a persistent database.
So to develop/debug the backend you do not have to run a local database, but you can if you want.
If you want to use a real database when debugging then you have to configure it in `Configuration.xml` by adjusting the `DatabasePersistenceConfiguration`-section accordingly:
In this `DatabasePersistenceConfiguration`-section you can edit the `DatabaseType`-value and the `DatabaseConnectionString`-value.
For running transient set `DatabaseType` to `Transient`.
(In this case `DatabaseConnectionString` is not used obviously.)
Apart from the transient-mode the supported values for `DatabaseType` are:

- `PostgreSQL`
- `MariaDB`

To reset all your local backend-configuration-values etc. to a plain state you can simply remove the entire `<repository-root>\OpenIMSBackend\Other\Workspace`-folder.

## Login and authorization

### How a request is authenticated

The login (`UserController.Login`) is the only business-endpoint which is reachable without an access-token, because it is what produces one.
Which routes are reachable unauthenticated is decided in exactly one place: `RoutesWhereUnauthenticatedAccessIsAllowed` in `Program.cs`.
Everything which is not listed there requires a valid access-token in the header `X-AccessToken`; the authentication-middleware resolves the user from it and rejects the request otherwise.

An unknown username and a wrong password are answered identically (401 with the message "Invalid credentials"), so that the answer does not reveal whether an account with that name exists.
A locked account is a different case and is answered with its own message, because at that point the caller has already proven that it knows the password of that account.

### How a request is authorized

The role which an endpoint requires is declared at the endpoint itself (`[Authorize(...)]`), so it is visible where the endpoint is defined and not in a separate table which can get out of sync.
`Authorize(RolenameUsers)` is what every logged-in user has, `Authorize(RolenameAdmins)` is what only an administrator has.

The role `Administrators` inherits the role `Users` (see `InitializationService`), so an administrator passes an `Authorize(RolenameUsers)`-check as well without having both roles assigned explicitly.

### Where the password is stored

Nowhere. Only the SHA-256-hash of it is stored in the column `Users.PasswordHash`.
The hashing happens in `PersistentAuthenticationService.Hash`, which is also what the login uses to compare, so there is exactly one place which decides how a password is hashed.

## Adding a SQL-statement

Every SQL-statement which the backend executes is an own embedded resource and is never written inline in C#.
Adding one therefore means three steps:

1. Add the method `GetScript<Name>()` to `Services/ISQLProvider.cs`.
2. Implement it in `Misc/SQLProvider.cs` by calling `this.LoadSQLScript("<Name>")`. The implementation is shared by all database-types, so this is done once.
3. Add the file `Resources/Database/<database-type>/Statements/<Name>.sql` **for every supported database-type**. A statement which is missing for one database-type is only noticed at runtime when exactly that statement is executed against exactly that database, so the testcase `DatabaseStructureTests` and the persistence-tests run against both real databases.

A parameter is referenced as `@Name` in the statement (this syntax works for PostgreSQL as well as for MariaDB) and is bound in the persistence with `GetParameter("Name", value)`.

## Adding a migration

A change of the database-schema is a new migration-file and never a change of an existing one, because an existing one has already been applied in every installation which runs.

Add `Resources/Database/<database-type>/Migrations/Migration<number>.sql` for every supported database-type, with the number continuing the existing ones (zero-padded to six digits).
The migrations are loaded from the embedded resources by their name (see `DatabaseInteractorPostgreSQL` and `DatabaseInteractorMariaDB`), so nothing has to be registered anywhere.

Keep in mind that MariaDB does not support transactional DDL-operations: a migration which fails halfway leaves the schema half-changed there, while PostgreSQL rolls it back.
Write a migration so that it does as little as possible per statement, and test it against both databases.
