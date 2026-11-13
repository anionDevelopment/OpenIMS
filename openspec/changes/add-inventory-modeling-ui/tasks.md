# Tasks

## 1. Backend: tree-DTO and business-logic

- [x] 1.1 Add `InventoryTreeNodeDTO` (`Id`, `Type`, `Name`, `Description`, `Children`) and `InventoryObjectDataDTO` (`Name`, `Description`) to `Model/DTOs` and verify the backend compiles
- [x] 1.2 Add `InventoryTreeNodeDTOMapper` in `Misc` which maps the locations of `GetAllLocations` to tree-nodes (location → buildings, areas; building → areas; area/container → containees) and add `GetInventoryTree()` to `IBusinessLogicService`/`BusinessLogicService`; verify with new testcases in `BusinessLogicServiceInventoryTests` (nested inventory, empty inventory, item with real place elsewhere is listed only at its target place)

## 2. Backend: endpoints

- [ ] 2.1 Add `GetInventoryTree`, `CreateLocation`, `CreateBuilding`, `CreateArea`, `CreateContainer`, `CreateItem`, `UpdateInventoryObject` and `DeleteInventoryObject` to `OpenIMSBackendController` with `[Authenticate]` and `[Authorize(RolenameUsers)]`; `CreateItem` uses `parentId` as target place and real place; verify the backend compiles and the testcases of the backend pass (`RunTestcases.py` of `OpenIMSBackend`)
- [x] 2.2 Add a testcase which verifies that `CreateItem` of the controller creates the item with its target place as real place (or the equivalent check of the business-call) and verify it passes
- [x] 2.3 Regenerate the api-specification and the frontend-client (`Build.py` of `OpenIMSBackend`, then `CommonTasks.py` of `OpenIMSFrontend`) and verify the new methods exist in `src/app/generated/open-ims-backend`

## 3. Frontend: page "Inventory"

- [x] 3.1 Add `InventoryObjectDialogComponent` (name, description; create disabled for an empty name) to the user-area and verify with a unit-test
- [x] 3.2 Add `InventoryComponent` (`mat-tree` with `childrenAccessor` and `expansionKey`, actions per type, reload after action, `actionFailed`-message) and verify with unit-tests (offered creations per type, reload keeps expansion, rejected action sets the flag)
- [x] 3.3 Add the route `user/inventory` with `authenticationCheckGuard` in `app.routes.ts` and the menu-entry "Inventory" in `user-area-container` (including `getSiteTitle`) and verify with the unit-test of `user-area-container`
- [ ] 3.4 Run the task `Translate` and verify the localized build succeeds
- [x] 3.5 Run `Linting.py` and `RunTestcases.py` of `OpenIMSFrontend` (without the visual-regression-part failing for unrelated pages) and verify both succeed

## 4. Visual-regression

- [x] 4.1 Add `e2e/UserInventoryPage.spec.ts` with a fixed simulated tree and expanded nodes, generate the baselines with `task uvrb` and verify them with `RunTestcases.py` of `OpenIMSFrontend` (all browsers, cross-browser checks included)

## 5. Documentation

- [x] 5.1 Update `ReadMe.md` (feature-list: inventory can be modeled via web-ui; remaining limitations), the feature-table in `Other/Reference/Reference.md` and the frontend-related reference/product-knowledge where pages are listed; verify every named path exists
- [x] 5.2 Add the accepted security-finding "every logged-in user can change the complete inventory" to the findings-table in `Other/Reference/Reference.md` and format the section per the `format-markdown-file`-skill
- [x] 5.3 Add an entry to the changelog in `Other/Resources/Changelog`

## 6. Integration

- [ ] 6.1 Run `scbuildcodeunits -c -v 4` and verify it succeeds without new linting-warnings

## Workflow follow-up

- Sync the delta-spec into `openspec/specs/inventory-modeling` and archive the change after review.
