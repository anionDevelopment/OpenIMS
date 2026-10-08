import { Page, test } from '@playwright/test';
import { expectCurrentPageToLookLikeBaseline, expectPageToLookLikeBaseline, openPageInReproducibleState } from './support/VisualRegression';
import { simulateLoggedInUser } from './support/AuthenticatedSession';
import { respondWith } from './support/SimulatedBackend';

/*
 * The group-management of the admin-area. It displays data which the backend delivers, so that data is answered
 * with fixed values here: otherwise the screenshot would depend on the content of a database.
 */
const groupsOfTheSimulatedInstallation: string[] = ['Administrators', 'Users', 'Workshop'];

const objectsOfTheSelectedGroup = [
    { id: '00000000-0000-0000-0000-000000000011', label: 'Building A' },
    { id: '00000000-0000-0000-0000-000000000012', label: 'Yard' }
];

const resultOfTheSearch = [
    { id: '00000000-0000-0000-0000-000000000021', label: 'Cupboard 1' },
    { id: '00000000-0000-0000-0000-000000000022', label: 'Room 101' },
    { id: '00000000-0000-0000-0000-000000000023', label: 'Store room' }
];

async function openPageWithSelectedGroup(page: Page): Promise<void> {
    await simulateLoggedInUser(page);
    await respondWith(page, '**/API/v0/UserController/GetAllRoles*', groupsOfTheSimulatedInstallation);
    await respondWith(page, '**/API/v0/OpenIMSBackend/GetAuthorizableItemsOfGroup*', objectsOfTheSelectedGroup);
    await respondWith(page, '**/API/v0/OpenIMSBackend/SearchInventoryObjects*', resultOfTheSearch);
    await openPageInReproducibleState(page, '/admin/groups');
    await page.getByRole('button', { name: 'Workshop' }).click();
    await page.getByText('Building A').waitFor();
    /*
     * The selected group is displayed by another button, which replaces the clicked one under the mouse. A button
     * creates the container of its ripple-effect (an element of its own) only when the mouse enters it or when it gets
     * the focus, and whether that happens for a button which appears under the resting mouse differs between the
     * browsers. Hovering it explicitly creates the container in every browser, so the layout is the same everywhere;
     * moving the mouse away afterwards removes the hover-effect from the screenshot.
     */
    await page.getByRole('button', { name: 'Workshop' }).hover();
    await page.mouse.move(0, 0);
}

test.describe('Admin-groups-list-page', () => {
    test('looks like the baseline-screenshot', async ({ page }) => {
        await simulateLoggedInUser(page);
        await respondWith(page, '**/API/v0/UserController/GetAllRoles*', groupsOfTheSimulatedInstallation);
        await expectPageToLookLikeBaseline(page, '/admin/groups', 'admin-groups-list-page');
    });

    test('looks like the baseline-screenshot when a group is selected', async ({ page }) => {
        await openPageWithSelectedGroup(page);
        await expectCurrentPageToLookLikeBaseline(page, 'admin-groups-list-page-group-selected');
    });

    test('looks like the baseline-screenshot when objects are searched', async ({ page }) => {
        await openPageWithSelectedGroup(page);
        await page.getByRole('combobox').click();
        await page.getByRole('option', { name: 'Store room' }).waitFor();
        await expectCurrentPageToLookLikeBaseline(page, 'admin-groups-list-page-object-search');
    });
});
