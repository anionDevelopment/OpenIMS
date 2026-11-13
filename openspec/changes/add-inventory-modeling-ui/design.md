# Design

## Context

- `IBusinessLogicService` already contains every operation which is needed (`CreateLocation`, `CreateBuilding`, `CreateArea`, `CreateContainer`, `CreateItem`, `UpdateInventoryObject`, `DeleteInventoryObject`, `GetAllLocations`), including all rules of `inventory-hierarchy` and their error-codes (404 for an unknown id, 400 for a rule-violation). They are tested in `BusinessLogicServiceInventoryTests`.
- `GetAllLocations` returns the business-types (`Location`, `Building`, `Area`, `Container`, `Item`). Their children are typed as interfaces (`IList<IContainee>`), and `Item.RealPlace` references another object of the graph.
- `OpenIMSBackendController` is the controller of the business-functionality. The frontend-client is generated from the api-specification of the backend.
- The frontend is built with NgModules; the user-area (`modules/user-area`) has the pages Dashboard and Settings, the frame `user-area-container` holds the user-menu. `groups-list` of the admin-area shows the established patterns: an `observer` which sets an `actionFailed`-flag, `confirm(...)` for a deletion, texts with `i18n`/`$localize`.

## Goals / Non-Goals

**Goals:**

- Expose the existing inventory-operations via http without duplicating any rule of the business-logic.
- A tree-page which supports creating, editing and deleting with as few clicks as possible.

**Non-Goals:**

- Moving buildings, areas, containers or items and changing the real place of an item.
- Access-checks of inventory-operations based on groups (see proposal).
- Loading the tree lazily or partially (see risks).

## Decisions

### A dedicated tree-DTO instead of serializing the business-types

The endpoint returns `InventoryTreeNodeDTO[]` (one per location) with `Id`, `Type` (the name of `InventoryObjectType`, for example `"Building"`), `Name`, `Description` and `Children` (recursive). The mapping from the business-types to the DTO lives in one static class `InventoryTreeNodeDTOMapper` in `Misc`, next to `InventoryTreeBuilder`, and is used by a new business-logic-method `GetInventoryTree()`.

Alternative: return `Location` directly. Rejected, because the children are typed as interfaces (the serializer only writes the members of the interface and the api-specification contains no type-information, so the generated client could not distinguish a container from an item) and because `Item.RealPlace` would serialize the referenced object a second time.

The type is transported as a string and not as an enum, because the existing search-endpoint (`SearchInventoryObjects`) uses the same names as strings, so the frontend uses one set of type-names.

### One endpoint per creatable type

`CreateLocation` (body `InventoryObjectDataDTO` with `Name` and `Description`), `CreateBuilding`, `CreateArea`, `CreateContainer`, `CreateItem` (each with the query-parameter `parentId` plus the same body). Each returns the id of the new object as `StringValueDTO`. Additionally `UpdateInventoryObject` (`PUT`, query `inventoryObjectId` + body) and `DeleteInventoryObject` (`DELETE`, query `inventoryObjectId`), and `GetInventoryTree` (`GET`).

Alternative: one generic `CreateInventoryObject` with the type as parameter. Rejected, because it would need a dispatch on the type in the controller, while the business-logic already offers one typed method per type; the controller stays a pure translation of http into one call. Ids are passed as query-parameters and data as body, as the existing endpoints of `OpenIMSBackendController` do.

`CreateItem` passes `parentId` as target place and as real place to `IBusinessLogicService.CreateItem` (spec: real place of a new item).

### Authorization with `[Authenticate]` and `[Authorize(Users)]`

Every new endpoint carries `[Authenticate]` and `[Authorize(CodeUnitSpecificConstants.RolenameUsers)]`. Every account has the role `Users` (administrators inherit it), so this is equivalent to "logged in", but it states explicitly which role is required, as the admin-endpoints do with `RolenameAdmins`. The business-logic-methods get no `requesterUserId`, because no rule depends on the requester.

### Tree with Angular Material `mat-tree`

The page `InventoryComponent` (`modules/user-area/inventory`, route `user/inventory`) uses `mat-tree` with `childrenAccessor` (nested data as it comes from the backend, no flattening) and `expansionKey` set to the id of the object, so that the expanded nodes stay expanded when the tree is reloaded after an action. `MatTreeModule` is part of `@angular/material`, which is already a dependency; no new dependency is added.

Alternative: a hand-written recursive template. Rejected, because `mat-tree` already provides expansion-state, keyboard-navigation and the tree-roles for accessibility.

Which children a type allows is defined once in the frontend as a map from type to creatable child-types (`Location → Building, Area`, `Building → Area`, `Area/Container → Container, Item`, `Item → none`). The backend enforces the same rules anyway; the map only decides which buttons are displayed.

### Dialog for name and description, `confirm` for deletion

Creating and editing open `InventoryObjectDialogComponent` (a `MatDialog` with the fields name and description, the create-button is disabled for a name which is empty or whitespace only). Deleting asks with `confirm(...)` like `groups-list` does, so that both pages behave the same. A rejected action sets an `actionFailed`-flag which displays a message, like in `groups-list`.

### Visual-regression

A new testcase `e2e/UserInventoryPage.spec.ts` answers `GetInventoryTree` with a fixed tree (one location with a building, a room, a box and an item, plus a yard) via `respondWith`, opens the page with `openPageInReproducibleState`, expands the nodes and compares with `expectCurrentPageToLookLikeBaseline`. Baselines are generated with `task uvrb` and verified with `RunTestcases.py`.

## Risks / Trade-offs

- [Every logged-in user can delete or rename any object of the inventory] → Deliberate decision of the product-owner (proposal). It is recorded as an accepted security-finding in `Other/Reference/Reference.md`, so that it is revisited when access-checks are specified. The deletion-rules of `inventory-hierarchy` (only empty objects) prevent deleting a whole subtree at once.
- [The page reloads the whole inventory after every action] → Same cause as the existing finding PER-1; acceptable for the expected size now. A partial reload would need new endpoints and is not part of this change.
- [The generated client changes] → Only new methods are added; existing method-names stay unchanged.
