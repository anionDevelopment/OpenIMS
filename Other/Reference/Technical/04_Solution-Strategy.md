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

## Inventory

### Purpose of the model

OpenIMS represents the physical world.
Every inventory-object is the representative of a real, physical thing:

- a `Location` (for example a company-site),
- a `Building`,
- an `Area`: a place where things are kept, either a room of a building or an area of a location which is not in a building (for example a yard or a terrace),
- a `Container` (anything which contains other things, for example a cupboard, a shelf or a euro-box),
- an `Item` (a single inventoried thing, for example a drill).

The model is designed so that it can only describe situations which are possible in the real world.

### Hierarchy

```text
Location
├── Building
│   └── Area (room)
│       └── Container / Item
└── Area (for example a yard)
    └── Container / Item

Container
└── Container / Item (to any depth)
```

### Composite-pattern

The containment of containers and items is implemented with the composite-pattern (`OpenIMSBackend/OpenIMSBackend/Model/BusinessTypes`):

- `IContainer` is the composite-role: it has the list `Containees` of the objects which it directly contains. `Area` and `Container` are containers.
- `IContainee` is the leaf-role: an object which can be contained. `Container` and `Item` are containees.
- `Container` is both, which is what allows nesting containers to any depth.
- `Location` and `Building` are neither containers nor containees: a location has the lists `Buildings` and `Areas`, a building has the list `Areas`.

Every inventory-object (`IInventoryObject`) is an `IAuthorizableItem` (see [Access to inventory-objects](#access-to-inventory-objects)).

See the [model-diagram](./05_Building-Block-View.md#model).

### Rules of the hierarchy

- A building is in exactly one location.
- An area is in exactly one location or building.
- A container or an item is in exactly one area or container.
- Ultimately every containee is in an area, every area is in a building or a location and every building is in a location.
- There are no cycles: a container can neither contain itself nor one of the containers which contain it.
- An item contains nothing.
- An object can only be deleted when it contains nothing and when no item actually is there, so that a deletion never leaves an object without a place and never deletes further objects implicitly. The groups assigned to the deleted object are removed together with it.

These rules are business-rules and are therefore checked in exactly one place: the inventory-part of `BusinessLogicService` (`BusinessLogicService.Inventory.cs`).
The persistence only stores the result.
An unknown id is answered with a `BadRequestException` with the status-code 404, a violation of a rule with a `BadRequestException` with the status-code 400.

### Target place and real place

For an item, two places are distinguished:

- The **target place** is the container (area or container) which contains the item in the hierarchy. It is the place where the item belongs to.
- The **real place** (`Item.RealPlace`, type `IRealPlace`) is the place where the item actually is.

Both are the same as long as the item is where it belongs.
Every `IContainer` (so every area and every container) is an `IRealPlace`; further kinds of real places (which are no containers) are planned and are the reason why `IRealPlace` is an interface of its own.
An item always has a real place.

The business-types do not have a reference to the object which contains them (no back-reference from a containee to its container).

### Access to inventory-objects

Every inventory-object is an `IAuthorizableItem`.
Groups can be assigned to an authorizable item; the members of an assigned group have read-access to it: they can see the object and what is in it (which items are where).
A group is a role of the GRYLibrary (`GRYLibrary.Core.APIServer.CommonDBTypes.Role`, table `Roles`), the same type which decides about the permissions of a user (for example `Users` and `Administrators`).

- Only the **direct** assignments are stored, in the m:n-table `Role_AuthorizableItems`, which contains nothing but the id of the role and the id of the authorizable item.
- The access is **inherited**: who has access to a location, a building, an area or a container has access to everything which is contained in it, directly or indirectly. This inheritance is **not stored** but computed at runtime (`BusinessLogicService.UserHasAccess`) by walking up the hierarchy from the requested object to its location. Therefore it always follows the current hierarchy, also after a move.
- The access is not inherited upwards: access to an item does not grant access to its container.
- The groups of a user include the groups which they inherit (for example `Administrators` inherits `Users`).
- The real place of an item does not influence the access; only the containment-hierarchy does.
- Only an administrator can create and delete groups and assign groups to an authorizable item or remove them.
- The groups which OpenIMS needs itself (`Users` and `Administrators`) and a group which another group inherits can not be deleted. Deleting a group removes its memberships and its assignments as well.

The access-rules are implemented in `BusinessLogicService.Authorization.cs`.

In the admin-area of the frontend, the page "Users" decides which user is member of which group and the page "Groups" creates and deletes groups and decides to which inventory-objects a group is assigned.

### Selection of an inventory-object

The inventory can be large, so a page never loads all inventory-objects to offer them for selection.
Instead the reusable component `app-inventory-object-selector` (module `SharedComponentsModule` of the frontend) is a dropdown with an inline-search:

- When the user stops typing for a moment, the entered text is sent to the backend (`SearchInventoryObjects`), so not every keystroke results in a request.
- The backend searches directly in SQL (statement `SearchInventoryObjects`): the name has to contain the text, ignoring the case; wildcard-characters of `like` in the text are escaped, so the text is taken literally.
- The search can be restricted to certain types (input `types` of the component), and the result is limited to 20 objects (`BusinessLogicService.MaximalAmountOfSearchResults`), ordered by name.
- The result is a list of pairs of id and label (`IdLabelDTO`); the label currently is the name of the object.
- The search is currently only allowed for administrators, because its result is not restricted to the objects the requesting user has access to.

### Modeling the inventory in the user-interface

The page "Inventory" of the user-area (`modules/user-area/inventory`, route `user/inventory`) displays the complete inventory as a tree (`mat-tree`) and offers on every object exactly the creations which the hierarchy allows for its type, plus editing and deleting.

- The backend delivers the tree with `GetInventoryTree` as `InventoryTreeNodeDTO`s (id, type, name, description, children). The business-types are not transported directly, because their children are typed as interfaces and `Item.RealPlace` references another object of the tree; the mapping is done by `InventoryTreeNodeDTOMapper`.
- There is one endpoint per creatable type (`CreateLocation`, `CreateBuilding`, `CreateArea`, `CreateContainer`, `CreateItem`) plus `UpdateInventoryObject` and `DeleteInventoryObject` in `OpenIMSBackendController`. Each calls exactly one method of the business-logic, so the rules of the hierarchy are not duplicated. A new item gets its target place as its real place.
- These endpoints require the role `Users`, so every logged-in user can use them, and they are not restricted by the groups which are assigned to inventory-objects (see finding `SEC-3`), because it is not specified yet which operation requires which access.
- After every action the whole tree is loaded again (see finding `PER-1`); the expansion of the tree is keyed by the id of the objects, so expanded objects stay expanded.

### Persistence of the inventory

All inventory-objects are stored in the one table `InventoryObjects`, one row per object, with the column `Type` (`Location`, `Building`, `Area`, `Container`, `Item`).
The hierarchy is stored as the reference `ParentId` to the containing object, the real place of an item as the reference `RealPlaceId`.
Because every row has exactly one parent, an object can not be in two places at once, already by the structure of the table.
Both references are foreign-keys to the same table, so a referenced object can not be deleted as long as it is referenced.

The persistence works with the flat `InventoryObjectRecord` (`Model/PersistenceTypes`).
`InventoryTreeBuilder` turns all records into the tree of business-types.
