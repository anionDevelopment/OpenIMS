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

The business-functionality of an inventory-management-system is not implemented yet.

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
