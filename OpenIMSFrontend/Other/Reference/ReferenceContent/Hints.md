# Hints

## Requirements

The following tools from the [tools-list](https://github.com/anionDev/ScriptCollection/blob/main/ScriptCollection/Other/Reference/ReferenceContent/Articles/RequirementsForCommonProjectStructure.md#Tools) are required to build this code-unit:

- `cyclonedx-npm`
- `docfx`
- `docker` (required by the visual-regression-tests)
- `git`
- `gitversion`
- `ng`
- `npm`
- `openapi-generator-cli`
- `python`
- `reportgenerator`
- `scriptcollection`

## IDE

The recommended IDE for this codeunit is [Visual Studio Code](https://code.visualstudio.com/).

## Structure

The application has three modules, which is also how the pages are grouped:

- `home-page`: everything which is reachable without being logged in. This is the login-mask (`login-form`), the frame which every page uses (`frame-work`, `footer`) and the hint-pages (not found, not authenticated, not authorized).
- `user-area`: the pages which every logged-in user reaches (dashboard, inventory, settings). The frame of these pages is `user-area-container`, which also contains the menu and the `user-icon` (the menu with the logout).
- `admin-area`: the pages which only an administrator reaches (the user-overview and the general settings). The frame of these pages is `admin-area-container`.

Which pages require a login is decided in exactly one place: `app.routes.ts`, where every route of the user- and the admin-area carries the `authenticationCheckGuard`.

## Login

The login-mask (`modules/home-page/login-form`) sends the credentials to the backend and stores the returned access-token via the `StorageService` in the session-storage of the browser.
Every following request sends that token as the header `X-AccessToken`; the generated api-client takes it as a parameter, so each call passes `storageService.getAccessToken()`.

A rejected login is shown as one unspecific hint, because the backend answers an unknown user and a wrong password identically on purpose.
The token is removed again in that case, so that a following request does not send a token which was never valid.

The token is held in the session-storage and not in the local-storage, so that closing the tab ends the session.

## Generated API-client

The folder `src/app/generated/open-ims-backend` is an auto-generated `typescript-angular`-client for the OpenIMSBackend-API and must not be edited manually (changes are overwritten on the next regeneration).
It is generated from the OpenAPI-specification of the backend: run `Build.py` in the backend-codeunit (this produces the spec) and then `CommonTasks.py` in this frontend-codeunit (this regenerates the client via `openapi-generator-cli`).

The generated method-names contain the major-version of the backend (for example `aPIV0UserControllerLoginPut` for `/API/v0/UserController/Login`), so a major-version-increase of the backend renames every method of the client.

Caveat when regenerating locally: the spec-generation of the backend uses `swagger tofile` against the built backend-assembly.
This step relies on the build-image (`SCBuilder`) and does not run against the GRYLibrary-APIServer-host in a plain local `dotnet build` in the `Development`-environment (the host validates its full service-graph on build, and the host-resolver of Swashbuckle falls back to looking for a `Startup`-type).
So an up-to-date client should be produced by the regular build (or in the build-image); after a change of the backend-API the client has to be regenerated rather than the endpoint-methods being written by hand.

## Unit-tests

The unit-tests are the `*.spec.ts`-files next to the sourcecode. They are executed by the angular unit-test-builder (`@angular/build:unit-test`) with [Vitest](https://vitest.dev/) as test-runner.

Things to know:

- The tests run in `jsdom` and not in a real browser, so no browser has to be installed on the build-host. A testcase which needs a real browser-feature does not belong here but into the visual-regression-tests.
- The globals of Vitest are enabled by the builder, so `describe`, `it` and `expect` do not have to be imported. The matchers are the ones of Vitest, which means for example `expect(x).toBe(true)` and `vi.fn()`.
- `vitest-base.config.ts` exists for exactly one reason: the builder does not offer an option for the output-folder of the coverage-report, and the build-pipeline expects the cobertura-file at `Other/Artifacts/TestCoverage`. The coverage-provider configured there has to stay `istanbul`, because that provider writes the cobertura-file with the structure the pipeline reads.
- Each configuration of the test-target builds against the build-configuration of the same name plus the build-configuration `Test`, so `npm run test-Development` tests the application with `environment.Development.ts`.
- The build-configuration `Test` exists only to set `aot: false`, and removing it breaks the testcases. The reason is not obvious: with ahead-of-time-compilation the bundler replaces `ngJitMode` by `false`, which drops the `ɵɵsetNgModuleScope`-calls from the bundle. An NgModule then has no declarations and no exports at runtime, so `TestBed.configureTestingModule({imports: [SomeModule]})` can not resolve the components that module exports and every element of them is reported as unknown (`NG0304`).

## Visual-regression-tests

Besides the unit-tests this codeunit contains visual-regression-tests which are implemented with [Playwright](https://playwright.dev/).
They open a page, take a screenshot of it and compare it with a baseline-screenshot.
The testcases are located in `e2e`, the configuration is located in `playwright.config.ts` and the baseline-screenshots are located in `Other/Resources/VisualRegressionBaselines`.

Things to know:

- The tests always run in a Linux-container and never directly on the host, see below.
- They are executed by `Other/QualityCheck/RunTestcases.py`, which means they are part of the build-script and of the pipeline.
- Playwright starts the application itself (using `npm run start`) inside that container.
- Every testcase is executed once per browser (`chromium`, `firefox` and `webkit`). The browser-loop is done by Playwright, so a testcase has to be written only once.
- A screenshot may differ from its baseline by a few pixels (the exact amount is documented in `playwright.config.ts`), because the rendering of a page is not bit-identical between two runs.
- Elements whose content changes without a change of the layout (for example the footer, which contains the version) are masked, otherwise every release would invalidate all baseline-screenshots.

### Why the tests run in a container

The rendering of a page depends on the operating-system: font-rendering and antialiasing, the available fonts and the rendering of form-controls and scrollbars differ between Windows, Linux and macOS.
The resulting difference is far bigger than the tolerance of the comparison, so a baseline-screenshot which was generated on one operating-system can not be compared with a screenshot which was generated on another one.

Therefore the tests are always executed in the playwright-container, which is a defined Linux-environment.
This way only one set of baseline-screenshots exists (`Other/Resources/VisualRegressionBaselines/<browser>/linux`) and a developer gets the same result as the pipeline.

The used image is defined in `.ScriptCollection/OCIImages/ImageDefinition.csv` of the repository, like the images of all other tools which are used by the build, and it is pulled automatically.
Its tag contains the playwright-version, which has to be the same version as the one of the `@playwright/test`-package in `package.json`, because the image contains the browsers of exactly that version.
The scripts check this and abort with a corresponding message if the versions do not match, so both have to be updated together.

### Updating the baseline-screenshots

After an intended change of the user-interface the baseline-screenshots have to be regenerated by running `task UpdateVisualRegressionBaselines` (or shorter: `task uvrb`) in the repository-folder.
This runs the same container and therefore produces the same screenshots the pipeline compares against.
The regenerated screenshots have to be reviewed before committing them, because this command accepts every change of the user-interface as the new expected state.

**The baseline-screenshots do not exist yet and have to be generated once with that command before the visual-regression-tests can pass.**

### Adding a testcase for another page

Step 1:

Create a file `e2e/<page>.spec.ts` which uses the existing helper-function.
Everything which is relevant for the comparison (tolerance, viewport, masked elements, browsers) is already defined centrally, so the testcase itself only names the route and the name of the baseline-screenshot:

```typescript
import { test } from '@playwright/test';
import { expectPageToLookLikeBaseline } from './support/VisualRegression';

test.describe('<page>', () => {
    test('looks like the baseline-screenshot', async ({ page }) => {
        await expectPageToLookLikeBaseline(page, '/<route>', '<name-of-the-baseline-screenshot>');
    });
});
```

The name of the baseline-screenshot has to be unique within the whole codeunit, because all baseline-screenshots of one browser are stored in one folder.
Do not add a loop over the browsers: the testcase is executed once per browser automatically.

Step 2:

Generate the baseline-screenshots of the new testcase by running `task UpdateVisualRegressionBaselines` (or shorter: `task uvrb`) in the repository-folder.
This creates one screenshot per browser in `Other/Resources/VisualRegressionBaselines/<browser>/linux`.

Step 3:

Check with `git status` that only the expected new screenshots were added and look at them to verify that they show the page in the expected state.
The command regenerates all baseline-screenshots, so a modification of an already existing screenshot means that the user-interface of another page has changed too.

Step 4:

Verify the new testcase by running `python RunTestcases.py` in `Other/QualityCheck`; it now has to pass in every browser.

### Testing a page which is only reachable after a login

The tests run without a backend, so a page behind the authentication-check-guard cannot simply be opened.
Instead the login is simulated: `e2e/support/AuthenticatedSession.ts` puts an access-token into the session-storage and answers the requests which the authentication-check performs with fixed responses.
A testcase for such a page therefore only needs one additional line:

```typescript
await simulateLoggedInUser(page);
```

`e2e/UserSettingsPage.spec.ts`, `e2e/UserDashboardPage.spec.ts` and the two admin-pages are examples for this.

The simulated user is an administrator and its name and id are fixed, because they are displayed in the user-interface and would otherwise change the screenshot.
A page which displays data of the backend (for example the user-overview of the admin-area) additionally needs the requests of that page to be answered, otherwise it is rendered empty; `e2e/AdminUsersListPage.spec.ts` shows how.
Answering them with fixed data is intended: it makes the screenshot independent of the content of a database.
Keep in mind that these tests therefore verify the user-interface only and not the interaction with the backend.

## Translations

The texts of the user-interface are marked with `i18n="@@<id>"` in the templates and are extracted into `Other/Resources/Translations/messages.xlf` by `ng extract-i18n`, which `Other/CommonTasks.py` (and therefore the build) runs.
`organize_translations` then makes sure that every locale which `angular.json` lists has a file.

A locale which is listed in `angular.json` but has no translation-file makes the localized build fail, so `angular.json`, `Other/Resources/Languages/Languages.txt` and the list in `Other/CommonTasks.py` have to stay in sync.
