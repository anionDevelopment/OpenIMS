# OpenIMS

[![CodeFactor](https://www.codefactor.io/repository/github/aniondev/OpenIMS/badge/main)](https://www.codefactor.io/repository/github/aniondev/OpenIMS/overview/main)
![Coverage backend](./OpenIMSBackend/Other/Resources/TestCoverageBadges/badge_shieldsio_linecoverage_blue.svg)
![Coverage frontend](./OpenIMSFrontend/Other/Resources/TestCoverageBadges/badge_shieldsio_linecoverage_blue.svg)

## Purpose

`OpenIMS` is an open source inventory-management-system.

## Quick-start

See [the reference of the OpenIMS-codeunit](./OpenIMS/Other/Reference/ReferenceContent/index.md).

## Technical product reference

See the general [product reference](./Other/Reference/Reference.md).

## Code-units

| Code-unit         | Purpose |
| ----------------- | ------- |
| `OpenIMS`         | The image-definition which packages backend and frontend into one OCI-image. |
| `OpenIMSBackend`  | The backend of OpenIMS. |
| `OpenIMSFrontend` | The web-frontend of OpenIMS. |

## Features

- ✅ Log in with username and password. A successful login returns an access-token which every following request carries in the header `X-AccessToken`; a logout invalidates it.
- ✅ Manage user-accounts: an administrator can create an account and can set the roles of an existing account.
- ✅ Role-based authorization. Every user has the role `Users`, an administrator additionally has `Administrators`, which inherits `Users`.
- ✅ Store the color-scheme (system, light or dark) per user, so the choice applies again after a new login and on another device.
- ✅ Support PostgreSQL and MariaDB as persistence, plus a transient (in-memory) persistence for testcases and demonstrations.
- ✅ Model the inventory as a hierarchy of locations, buildings, areas, containers and items in the backend, including the distinction between the place where an item belongs to and the place where it actually is (see [Inventory-model](#inventory-model)).
- ✅ Grant groups read-access to parts of the inventory; the access applies to everything which is contained. An administrator manages the groups (create, delete, assign inventory-objects) on the page "Groups" of the admin-area and the members of the groups on the page "Users".
- ✅ Model the inventory on the page "Inventory" of the user-area: every logged-in user sees the complete inventory as a tree and creates locations, buildings, areas, containers and items, changes their name and description and deletes them (see [Inventory-model](#inventory-model)).
- ❌ Moving inventory-objects and changing the real place of an item are not available via endpoints and user-interface yet.
- ❌ The operations of the inventory do not check the access of the requesting user yet (the access can already be computed, but it is not specified yet which operation requires which access). Until then every logged-in user sees and changes the complete inventory, regardless of the groups.

## Inventory-model

OpenIMS represents the physical world: every object of the inventory-model stands for a real, physical thing.

| Type        | Represents | Examples |
| ----------- | ---------- | -------- |
| `Location`  | A site which contains buildings and areas. | A company-site, a property. |
| `Building`  | A building. It is always in exactly one location and consists of areas. | An office-building, a warehouse. |
| `Area`      | A place where things are kept: a room of a building, or an area of a location outside of a building. | A room, a yard, a terrace. |
| `Container` | Anything which contains other things. It is in an area or in another container. | A cupboard, a shelf, a euro-box. |
| `Item`      | A single thing which is inventoried. It is in an area or in a container. | A drill, a laptop. |

The model is a strict hierarchy (location → building → area → container → … → item, and location → area), just as in the physical world:

- Everything is in exactly one place: a building in one location, an area in one location or building, a container or an item in one area or container.
- Ultimately every container and every item is in an area, every area is in a building or a location, and every building is in a location.
- Nothing can be in itself, neither directly nor indirectly.

For an item, OpenIMS distinguishes two places:

- The **target place** is the building or container which contains the item in the model: where the item belongs to.
- The **real place** is where the item actually is at the moment (for example because it was put back in the wrong cupboard or is temporarily used in another room). Currently every area and every container can be a real place; further kinds of real places are planned.

Both are the same as long as the item is where it belongs.

Access to the inventory is granted to groups: a group can be assigned to any location, building, area, container or item, and its members then have read-access to that object and to everything which is contained in it (they can see which items are where).

See the [technical documentation](./Other/Reference/Technical/04_Solution-Strategy.md#inventory) for details.

## Getting Started

### Usage

TODO

## Build

This product requires to use `scbuildcodeunits` implemented/provided by [ScriptCollection](https://github.com/anionDev/ScriptCollection) to build the project.

## Changelog

See the [Changelog-folder](./Other/Resources/Changelog).

## Contribute

Contributions are always welcome.

See [Contributing.md](./Contributing.md) for information about that.

## Repository-structure

This product uses the [CommonProjectStructure](https://projects.aniondev.de/PublicProjects/Common/ProjectTemplates/-/blob/main/Conventions/RepositoryStructure/CommonProjectStructure/CommonProjectStructure.md) as repository-structure.

## Branching-system

This product follows the [GitFlowSimplified](https://projects.aniondev.de/PublicProjects/Common/ProjectTemplates/-/blob/main/Conventions/BranchingSystem/GitFlowSimplified/GitFlowSimplified.md)-branching-system.

## Versioning

This product follows the [SemVerPractise](https://projects.aniondev.de/PublicProjects/Common/ProjectTemplates/-/blob/main/Conventions/Versioning/SemVerPractise/SemVerPractise.md)-versioning-system.

## License

See [License.txt](./License.txt).
