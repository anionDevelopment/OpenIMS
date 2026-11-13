# OpenIMS

OpenIMS is an open source inventory-management-system.

## Technical documentation

- [1. Introduction and Goals](./Technical/01_Introduction-and-Goals.md)
- [2. Constraints](./Technical/02_Constraints.md)
- [3. Context and Scope](./Technical/03_Context-and-Scope.md)
- [4. Solution Strategy](./Technical/04_Solution-Strategy.md)
- [5. Building Block View](./Technical/05_Building-Block-View.md)
- [6. Runtime View](./Technical/06_Runtime-View.md)
- [7. Deployment View](./Technical/07_Deployment-View.md)
- [8. Crosscutting Concepts](./Technical/08_Crosscutting-Concepts.md)
- [9. Architectural Decisions](./Technical/09_Architectural-Decisions.md)
- [10. Quality Requirements](./Technical/10_Quality-Requirements.md)
- [12. Glossary](./Technical/12_Glossary.md)

## Responsibilities

| Responsibility  | Name and contact-information |
| --------------- | ---------------------------- |
| Product-owner   | Marius Göcke                 |
| Product-manager | Marius Göcke                 |
| Support         | Bugs and feature-requests: Use GitHub-issues<br>Support for issues which are not in the responsibility of the developer: Support-contracts on demand |

## License & Pricing

See [License.txt](https://github.com/anionDevelopment/OpenIMS/License.txt).
(For pricing of support-contracts please contact the product-owner.)

## External resources

- [Repository](https://github.com/anionDevelopment/OpenIMS)

## Features

This table lists the features which a user can expect from this product.
See the specification in `openspec/specs/product-features/spec.md` for how this list is maintained.

| id      | state       | description                                                                                                                                                                                       |
| ------- | ----------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| feat-1  | available   | Log in with a username and a password; a logout invalidates the access-token which the login returned.                                                                                            |
| feat-2  | available   | An administrator manages user-accounts: creating an account and setting the roles of an existing account.                                                                                         |
| feat-3  | available   | Role-based authorization: every user has the role `Users`, an administrator additionally has `Administrators`, which inherits `Users`.                                                            |
| feat-4  | available   | Store the color-scheme (system, light or dark) per user, so the choice applies again after a new login and on another device.                                                                     |
| feat-5  | available   | Choose the persistence: PostgreSQL, MariaDB or a transient in-memory persistence for testcases and demonstrations.                                                                                |
| feat-6  | available   | Model the inventory as a hierarchy of locations, buildings, areas, containers and items, including the distinction between the place where an item belongs to and the place where it actually is. |
| feat-7  | available   | Grant groups read-access to parts of the inventory and manage the groups and their members as an administrator; access to a place grants access to everything which is contained in it.           |
| feat-8  | available   | Model the inventory on the page "Inventory": every logged-in user sees the complete inventory as a tree and creates, renames and deletes locations, buildings, areas, containers and items.       |
| feat-9  | unavailable | Check the access of the requesting user on the operations of the inventory (which operation requires which access).                                                                               |
| feat-10 | unavailable | Move inventory-objects and change the real place of an item through endpoints and the user-interface.                                                                                             |

## Findings

Every finding of a code-review, a security-check, a bug-search, a performance-analysis or any other analysis of this repository is collected in the following table.
See the specification in `openspec/specs/common-projects-structure-architecture/spec.md` for how findings are handled.

| Id    | Priority  | State    | Description                                                                                                                                                                                                                                                                                                                     | Impact                                                                                                                                                                             | Notes                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| ----- | --------- | -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| SEC-1 | mid       | open     | The maintenance-endpoints `/API/Other/Maintenance/Metrics` and `/API/Other/Maintenance/CurrentVersion` are available without authentication.                                                                                                                                                                                    | Security: anyone who can reach these endpoints can read the metrics and the current version; the metrics can expose operational information.                                       | By design, because only human users are authenticated while a metrics-scraper is a technical user. Intended mitigation: protect these endpoints with the reverse-proxy using basic-auth. The endpoints can be enabled or disabled with a commandline-switch on initial-configuration-generation or later in the configuration-file (`InitialEnableEndpointMetricsValue`, `InitialEnableEndpointCurrentVersionValue`, both default to disabled). Querying the current version is not necessarily a weakness but can be disabled for hardening. |
| SEC-2 | very high | open     | If `InitialAdminPassword` is not passed on the first start, the password of the admin-user defaults to `admin` (`InitializationService.Initialize`).                                                                                                                                                                            | Security: a well-known default credential lets anyone who knows it log in as administrator until the password is changed.                                                          | Change it, or pass a real password on the first start, before the installation is reachable from a network which is not under your control.                                                                                                                                                                                                                                                                                                                                                                                                   |
| SEC-3 | high      | accepted | Every logged-in user can read, create, rename and delete every inventory-object through the endpoints of the page "Inventory" (`GetInventoryTree`, `Create...`, `UpdateInventoryObject`, `DeleteInventoryObject` in `OpenIMSBackendController`); the groups which grant read-access to inventory-objects are not applied there. | Security: every account sees the complete inventory and can change it or delete it object by object (only empty objects can be deleted), regardless of the groups it is member of. | Deliberate decision of the product-owner (change `add-inventory-modeling-ui`), because it is not specified yet which inventory-operation requires which access. Has to be revisited when that is specified (feature `feat-9`).                                                                                                                                                                                                                                                                                                                |
| PER-1 | mid       | open     | `IBusinessLogicService.GetLocation` and `GetAllLocations` load all inventory-objects and build the complete tree, because the real place of an item can be in another location.                                                                                                                                                 | Performance: the effort grows with the size of the whole inventory and not with the size of the requested part, so reading becomes slow for a large inventory.                     | Simple and correct for now. If the inventory becomes large, reading has to be restricted, for example by loading only a subtree and resolving the real places separately. Classified as technical debt in the original documentation.                                                                                                                                                                                                                                                                                                         |
| PER-2 | mid       | open     | `IBusinessLogicService.UserHasAccess` reads the assigned groups of every object from the requested object up to its location, one query per level.                                                                                                                                                                              | Performance: checking the access for many objects at once (for example for a list) produces many queries.                                                                          | Fast for the usual depth of the hierarchy. Checking the access for many objects at once should be done with one query instead. Classified as technical debt in the original documentation.                                                                                                                                                                                                                                                                                                                                                    |
