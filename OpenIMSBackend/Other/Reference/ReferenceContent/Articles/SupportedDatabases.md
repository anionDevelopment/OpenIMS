# Supported databases

Currently the following databases are supported:

- MariaDB
- PostgreSQL
- Transient

## Transient

"Transient" is a kind of persistence for testing- and demonstration-purposes.
There are usually 2 usecases for that:

- Accelerate testcases which need a persistence but not necessarily a database-persistence.
- Running the backend without the requirement of having a database.

From the perspective of the usage (as a backend-developer) Transient is similar to an in-memory-database which is internally included in the backend and does not have any third-party-dependencies.
The persisted data will always be removed when the backend gets stopped when using the transient-persistence.
So Transient is not appropriate for productive-usage for obvious reasons.

## How a database-type is added

The backend does not contain any SQL in its C#-code: every statement is an embedded resource under `Resources/Database/<database-type>/Statements` and is loaded through `ISQLProvider`, and the schema is created by the migration-scripts under `Resources/Database/<database-type>/Migrations`.

Supporting a further database-type therefore means:

1. Add a folder of statements and a folder of migrations for it.
2. Add an `ISQLProvider`-implementation which points to the new statement-folder (this is a one-line class, see `SQLProviderPostgreSQL`).
3. Add an `IOpenIMSDatabaseInteractor`-implementation which points to the new migration-folder (see `DatabaseInteractorPostgreSQL`).
4. Register both in `Program.cs` and in `GetOpenIMSDatabaseInteractorVisitor`.

The business-logic and the persistence itself stay unchanged, which is the point of this structure.

## Known difference between the supported databases

MariaDB does not support transactional DDL-operations, PostgreSQL does.
A migration which fails halfway therefore leaves the schema half-changed on MariaDB while it is rolled back completely on PostgreSQL.
This is the reason why the testcases run against both databases and not only against one of them.
