---
name: product-knowledge
description: What OpenIMS is, how this repository is structured and which mechanisms exist for building, testing and visual-regression-testing it. Use this before fixing a defect or developing a feature in this repository, to know where things belong and how to verify a change.
---

# OpenIMS

OpenIMS is an **inventory-management-system**. It is meant to keep track of the items an organisation owns:
what exists, how much of it, where it is and who is responsible for it.

## State of the product

The business-functionality of the inventory-management is **not implemented yet**. What exists is the whole
technical foundation plus exactly one mechanism which every further feature needs:

- Logging a user in and out (username/password against an access-token).
- Managing user-accounts and their roles (an administrator creates accounts and assigns roles).
- One installation-wide setting (whether new accounts may be created) and one per-user setting (the
  color-scheme).

So when a feature of the inventory-management is asked for, it is a new feature and not a change of an existing
one. The places where it belongs are named below.

## Structure of the repository

The repository follows the "common project structure": all sourcecode lives in code-units, and every code-unit
has its own `Other`-folder with its build-, quality-check- and reference-files. Use the
`work-with-common-project-structure`-skill when you need the details of that structure.

The code-units are:

- `OpenIMSBackend` — the dotnet-api-server. It owns the users and is the only part which talks to the database.
- `OpenIMSFrontend` — the angular-web-application. It only talks to the backend.
- `OpenIMS` — the composition of the parts: the container-image which contains backend and frontend behind an
  internal nginx, plus the example-deployment.

The product-version is the version of every code-unit; it is `0.1.0` as long as the repository has no git-tag.
Because the major-version is `0`, every api-route lives under `/API/v0/...` and the generated frontend-client
names its methods accordingly (`aPIV0UserControllerLoginPut`). A major-version-increase renames all of them.

### Where things belong in the backend

`OpenIMSBackend/OpenIMSBackend/`:

- `Controller/UserController.cs` — everything about the account of the caller itself (login, logout, own
  profile, own settings) and the administrative user-management.
- `Controller/OpenIMSBackendController.cs` — **the controller of the business-functionality.** The endpoints of
  the inventory-management belong here. It currently only holds the general settings.
- `Services/IBusinessLogicService.cs` + `BusinessLogicService.cs` — **all business-rules.** A controller only
  translates http to a call of this service; who is allowed to do what and what a valid value is, is decided
  here exactly once.
- `Services/IPersistence.cs` — the persistence-interface. It is implemented three times:
  `DatabasePersistence` (for a real database), `TransientPersistence` (in-memory) and — for everything about
  users, roles and access-token — the transient authentication-persistence of the GRYLibrary, to which
  `TransientPersistence` delegates.
- `Services/ISQLProvider.cs` + `Misc/SQLProvider.cs` — every SQL-statement the backend executes. The class
  contains no SQL: each statement is an embedded resource.
- `Resources/Database/<MariaDB|PostgreSQL>/Statements/*.sql` and `.../Migrations/*.sql` — the statements and the
  schema, once per supported database-type.
- `Program.cs` — the composition-root: middleware, which persistence is used, which services are registered.

### The two rules which are easy to get wrong in the backend

1. **A new SQL-statement has to be added for every supported database-type.** Add the method to `ISQLProvider`,
   implement it once in `Misc/SQLProvider.cs` (`LoadSQLScript("<Name>")`) and add
   `Resources/Database/<type>/Statements/<Name>.sql` for **both** MariaDB and PostgreSQL. A statement which is
   missing for one type only fails at runtime against exactly that database, which is why the testcases run
   against both.
2. **A schema-change is a new migration-file, never a change of an existing one**, because an existing one has
   already been applied in every running installation. Add
   `Resources/Database/<type>/Migrations/Migration<number>.sql` for both types; nothing has to be registered.
   Keep in mind that MariaDB does not support transactional DDL, so a migration which fails halfway leaves the
   schema half-changed there while PostgreSQL rolls it back.

### Where things belong in the frontend

`OpenIMSFrontend/src/app/modules/` has three modules, which is also how the pages are grouped:

- `home-page` — everything reachable without being logged in: the login-mask (`login-form`), the frame of every
  page (`frame-work`, `footer`) and the hint-pages.
- `user-area` — the pages every logged-in user reaches. Their frame is `user-area-container`.
- `admin-area` — the pages only an administrator reaches. Their frame is `admin-area-container`.

Which pages require a login is decided in exactly one place: `app.routes.ts`, where every route of the user- and
the admin-area carries the `authenticationCheckGuard`.

The access-token is held by `services/storage.service.ts` in the **session**-storage (so closing the tab ends
the session) and is passed to every generated api-method as a parameter.

`OpenIMSFrontend/src/app/generated/open-ims-backend` is generated from the api-specification of the backend.
**Never edit anything in it**: the next build overwrites it. If something is wrong there, fix it in the backend
and regenerate. To regenerate only, run `Build.py` of the backend-code-unit and afterwards `CommonTasks.py` of
the frontend-code-unit; a full `task bb` also does it but takes far longer.

## Building

`scbuildcodeunits` builds everything: compile, unit-tests, linting, security-checks and the
visual-regression-tests. The task `task bb` (`BaseBuildAllCodeunits`) does the same. Both take a while — half an
hour is normal. `--fastlane` skips the testcases.

For a single code-unit, run the scripts in `<code-unit>/Other/Build` and `<code-unit>/Other/QualityCheck`, and
run them from the folder they are located in.

Two things are generated at repository-level and are not in git: the certificate-authority and the
development-certificate (`Other/Resources/CA`, `Other/Resources/DevelopmentCertificate`). Without them the
backend-code-unit cannot even do its common-tasks. They are created by
`TFCPS_Tools_General.prepare_building_codeunits(<repository-folder>, False, True)`, which also generates
`Taskfile.yml` from `OpenIMS.code-workspace`, the code-units-overview-diagram and the svg-files of all
plantuml-diagrams. **`Taskfile.yml` is generated** — change the workspace-file, not the taskfile.

## Testing

- **Backend unit-tests** (`TestKind=UnitTest`) need nothing but the code. They cover the business-logic against
  the transient persistence.
- **Backend integration-tests** (`TestKind=IntegrationTest`) need a **real database in a container**, see
  below.
- **Frontend unit-tests** are the `*.spec.ts`-files next to the sourcecode, executed with Vitest in `jsdom`.
- **Visual-regression-tests** check that the rendered pages do not change unintentionally, see below.

### The database-tests and how they use containers

The integration-tests of the backend do not mock the database: they run against a real PostgreSQL and a real
MariaDB, each in a container.

- `TestUtilities/DatabaseTestFrameworkForPostgreSQL.cs` and `...ForMariaDB.cs` start and stop that container by
  calling the tasks `LocaltestservicePostgresqlStart` / `LocaltestserviceMariadbStart` of `Taskfile.yml`, which
  run the docker-compose-files under `Other/Resources/LocalTestServices/`. The framework waits until the
  container reports itself healthy and until a connection really works.
- The containers attach to the **external docker-network `openims_net`**. It is not created by the
  compose-files, so it has to exist: `docker network create openims_net`.
- `TestUtilities/Utilities.cs` decides the host of the connection-string: inside the build-container the
  test-process is attached to that network and reaches the database under its container-name
  (`openims_database`), locally the ports are published and it is `localhost`. That is what the environment-
  variable `ISRUNNINGINBUILDCONTAINER` is for.
- **Every testcase which touches a database takes `Utilities.LockForTests`**, because all of them share one
  container. Without it two testcases would reset the database under each other.
- Each testcase resets the database and runs the migrations, so it starts from a defined state.

The testcases themselves are written **once** and executed **once per implementation**:

- `Testcases/Services/PersistenceTests/PersistenceTestsBase.cs` holds the testcases which every
  `IPersistence`-implementation has to fulfill. `PersistenceTestsForTransient`, `...ForPostgreSQL` and
  `...ForMariaDB` execute them. This is what keeps the three implementations behaving the same.
- `Testcases/Services/DatabaseTests/DatabaseTestsBase.cs` holds the testcases which only make sense against a
  real database (the migrations run, they are idempotent, a user and an access-token survive a round-trip, the
  role-inheritance is stored). `DatabaseTestsForPostgreSQL` and `...ForMariaDB` execute them.
- `Testcases/Services/DatabaseStructureTestsFor<type>.cs` runs all migrations and then generates the
  entity-relationship-diagram of the resulting schema **by reading it back from the database**, into
  `Other/Reference/ReferenceContent/Articles/Diagrams/DatabaseStructure.<type>.plantuml`. So the diagram in the
  reference is not written by hand; it is a test-artifact.

When adding a testcase to one of those base-classes, add the abstract method as well and the override in every
derived class — otherwise it silently runs for only one implementation.

### The testcases which start a real server and the test-coverage

`Testcases/Controller/UserControllerTests.cs` and `MaintenanceControllerTests.cs` start a real server in the
test-process (`TestUtilities/IntegrationTestFramework.cs`). They are part of the normal test-run; no testcase of
this codeunit is excluded.

Know this before debugging a coverage-failure of the build: such a testcase interferes with the collected
test-coverage. Measured on 2026-09-22, a run of the unit-tests alone reports a line-rate of `0.2176`, a run of
the server-starting class alone reports `0`, and a run which contains both reports `0` — so such a testcase does
not just contribute nothing, it zeroes the merged report. The build rejects a coverage of 0%
(`__check_testcoverage` in ScriptCollection), so `RunTestcases.py` can fail with
"The test-coverage of package 'OpenIMSBackend' must be greater than 0%" although every testcase passed.

If that happens, the cause is the coverage-collection and not a failing testcase. To see the real coverage of a
run, exclude the server-starting testcases:

```
dotnet test OpenIMSBackend.sln -c QualityCheck --filter "TestKind=UnitTest" --settings runsettings.xml
```

Two further properties of that framework are worth knowing before writing such a testcase:

- A server started by a testcase runs in the execution-mode `TestRun` (only `--RealRun true` enters the
  production-path). In that mode the initialization-service does not run, so no roles and no admin-user exist
  and **nobody can log in**. Anything which needs a logged-in user belongs into the unit-tests of the
  business-logic.
- `IntegrationTestConfiguration` has to be constructed with `runInOwnThread: true`. With `false` the call which
  starts the server does not return and the testcase hangs.

Two differences between the implementations are real and are documented in the testcases, do not "fix" them in
the tests:

- Which exception an unknown user or role causes is not part of the contract (the database-implementation throws
  `KeyNotFoundException`, the transient one lets the lookup of its collection fail).
- In the transient implementation an access-token is resolved to its user through the token-collection **of the
  user**, so a token has to be registered both at the user and in the persistence — which is exactly what
  `PersistentAuthenticationService.Login` does.

### Visual-regression-tests

They live in `OpenIMSFrontend/e2e` and are written with playwright.

- **They always run in a container** (`mcr.microsoft.com/playwright`, the version is defined in
  `.ScriptCollection/OCIImages/ImageDefinition.csv` and has to match the `@playwright/test`-version of
  `package.json`). A screenshot depends on the operating-system (font-rendering, available fonts, rendering of
  form-controls), and that difference is far bigger than the tolerance of the comparison. Therefore only
  linux-baselines exist, under `OpenIMSFrontend/Other/Resources/VisualRegressionBaselines/<browser>/linux/`.
- **A testcase is written once and executed by three browsers** (chromium, firefox, webkit). Every browser has
  its own baseline-screenshot.
- **Generate baselines**: `task uvrb` (or `UpdateVisualRegressionBaselines.py` in
  `OpenIMSFrontend/Other/QualityCheck`). Always run `RunTestcases.py` afterwards: newly generated baselines say
  nothing about whether the page is rendered reproducibly, and only the comparison shows that.
- **No backend is required.** The testcases answer the requests of the application themselves
  (`e2e/support/SimulatedBackend.ts`, `e2e/support/AuthenticatedSession.ts`), so the screenshots do not depend
  on the content of a database. Everything a page displays has to be a fixed value; anything which differs per
  run has to be masked. A page behind the login is reached by simulating the logged-in state
  (`simulateLoggedInUser`) and not by a real login; the simulated user is an administrator with a fixed name and
  id. A page which displays data of the backend additionally needs those requests answered, otherwise it
  renders empty — `e2e/AdminUsersListPage.spec.ts` shows how.
- **`expectPageToLookLikeBaseline`** in `e2e/support/VisualRegression.ts` is the entry-point of every testcase.
  It opens the page, waits until it is really rendered and compares it. Tolerance, viewport, timezone and
  color-scheme are defined centrally in `playwright.config.ts`. The footer (which contains the version) is
  masked on every page.
- **Two further checks compare the browsers with each other** (`check_screenshots_are_similar` and
  `check_layouts_are_similar`, called by `RunTestcases.py`). A baseline-screenshot is only ever compared with
  the screenshot of its own browser, so a page which looks different in one browser than in the others would
  stay unnoticed forever. The second of the two compares the geometry (position and size of every element,
  written by `e2e/support/PageLayout.ts` while the tests run) and not the pixels: the engines rasterize text
  differently, so even a page which looks identical differs in thousands of pixels.

If one of those two checks reports a difference, that is a defect of the page and not of the testcase.

## Things which are easy to get wrong

- **The lint-configuration of the frontend really lints.** `angular.json` points at `src/app/**` (not at
  `src/**` subfolders which do not exist), the generated api-client is excluded in `eslint.config.js`, and
  `@angular-eslint/prefer-standalone` is switched off there **on purpose**: this application is deliberately
  built with NgModules, so every component opts out of standalone.
- **The fonts are delivered with the application** (`@fontsource`-packages, see `angular.json`) and are not
  loaded from an external font-provider. Do not change that back: an externally loaded font makes the browser
  render with a replacement-font first and repaint afterwards, which makes visual-regression-testcases flaky,
  and it makes every client contact a third party. The license-texts are in `OpenIMSFrontend/public/Licenses`.
- **A locale which `angular.json` lists needs a translation-file**, otherwise the localized build fails.
  `angular.json`, `Other/Resources/Languages/Languages.txt` and the locale-list in `Other/CommonTasks.py` have
  to stay in sync.
- **The build-configuration `Test` of the frontend exists only to set `aot: false`** and must not be removed:
  with ahead-of-time-compilation the bundler drops the module-scopes, so `TestBed` cannot resolve the components
  which an imported NgModule exports and reports every element as unknown (`NG0304`).
- **The password is never stored**, only its hash (`PersistentAuthenticationService.Hash`). An unknown user and
  a wrong password are answered **identically** so that the answer does not reveal which usernames exist — keep
  that when touching the login.
