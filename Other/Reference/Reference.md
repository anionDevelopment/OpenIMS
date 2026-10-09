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

## Findings

Every finding of a code-review, a security-check, a bug-search, a performance-analysis or any other analysis of this repository is collected in the following table.
See the specification in `openspec/specs/common-projects-structure-architecture/spec.md` for how findings are handled.

| Id    | State | Description                                                                                                                                                                     | Impact                                                                                                                                                         | Notes                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| ----- | ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| SEC-1 | open  | The maintenance-endpoints `/API/Other/Maintenance/Metrics` and `/API/Other/Maintenance/CurrentVersion` are available without authentication.                                    | Security: anyone who can reach these endpoints can read the metrics and the current version; the metrics can expose operational information.                   | By design, because only human users are authenticated while a metrics-scraper is a technical user. Intended mitigation: protect these endpoints with the reverse-proxy using basic-auth. The endpoints can be enabled or disabled with a commandline-switch on initial-configuration-generation or later in the configuration-file (`InitialEnableEndpointMetricsValue`, `InitialEnableEndpointCurrentVersionValue`, both default to disabled). Querying the current version is not necessarily a weakness but can be disabled for hardening. |
| SEC-2 | open  | If `InitialAdminPassword` is not passed on the first start, the password of the admin-user defaults to `admin` (`InitializationService.Initialize`).                            | Security: a well-known default credential lets anyone who knows it log in as administrator until the password is changed.                                      | Change it, or pass a real password on the first start, before the installation is reachable from a network which is not under your control.                                                                                                                                                                                                                                                                                                                                                                                                   |
| PER-1 | open  | `IBusinessLogicService.GetLocation` and `GetAllLocations` load all inventory-objects and build the complete tree, because the real place of an item can be in another location. | Performance: the effort grows with the size of the whole inventory and not with the size of the requested part, so reading becomes slow for a large inventory. | Simple and correct for now. If the inventory becomes large, reading has to be restricted, for example by loading only a subtree and resolving the real places separately. Classified as technical debt in the original documentation.                                                                                                                                                                                                                                                                                                         |
| PER-2 | open  | `IBusinessLogicService.UserHasAccess` reads the assigned groups of every object from the requested object up to its location, one query per level.                              | Performance: checking the access for many objects at once (for example for a list) produces many queries.                                                      | Fast for the usual depth of the hierarchy. Checking the access for many objects at once should be done with one query instead. Classified as technical debt in the original documentation.                                                                                                                                                                                                                                                                                                                                                    |
