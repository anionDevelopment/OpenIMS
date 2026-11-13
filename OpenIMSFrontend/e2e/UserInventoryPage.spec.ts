import { Page, test } from '@playwright/test';
import { expectCurrentPageToLookLikeBaseline, expectPageToLookLikeBaseline, openPageInReproducibleState } from './support/VisualRegression';
import { simulateLoggedInUser } from './support/AuthenticatedSession';
import { respondWith } from './support/SimulatedBackend';

/*
 * The page for modeling the inventory. It displays the inventory-tree which the backend delivers, so the tree is
 * answered with fixed values here: otherwise the screenshot would depend on the content of a database.
 */
const inventoryOfTheSimulatedInstallation = [
    {
        id: '00000000-0000-0000-0000-000000000101', type: 'Location', name: 'Headquarter', description: 'The main site', children: [
            {
                id: '00000000-0000-0000-0000-000000000111', type: 'Building', name: 'Building A', description: '', children: [
                    { id: '00000000-0000-0000-0000-000000000121', type: 'Area', name: 'Archive', description: '', children: [] },
                    {
                        id: '00000000-0000-0000-0000-000000000122', type: 'Area', name: 'Room 101', description: 'Workshop', children: [
                            {
                                id: '00000000-0000-0000-0000-000000000131', type: 'Container', name: 'Euro-box', description: '', children: [
                                    { id: '00000000-0000-0000-0000-000000000141', type: 'Item', name: 'Drill', description: 'Cordless drill', children: [] }
                                ]
                            }
                        ]
                    }
                ]
            },
            { id: '00000000-0000-0000-0000-000000000123', type: 'Area', name: 'Yard', description: '', children: [] }
        ]
    },
    {
        id: '00000000-0000-0000-0000-000000000102', type: 'Location', name: 'Warehouse', description: '', children: [
            { id: '00000000-0000-0000-0000-000000000112', type: 'Building', name: 'Hall 1', description: '', children: [] }
        ]
    }
];

async function openInventoryPage(page: Page): Promise<void> {
    await simulateLoggedInUser(page);
    await respondWith(page, '**/API/v1/OpenIMSBackend/GetInventoryTree*', inventoryOfTheSimulatedInstallation);
}

async function expand(page: Page, nameOfTheObject: string): Promise<void> {
    const node = page.locator('mat-tree-node').filter({ has: page.locator('.name').getByText(nameOfTheObject, { exact: true }) });
    await node.getByRole('button', { name: 'Expand or collapse' }).click();
}

test.describe('User-inventory-page', () => {
    test('looks like the baseline-screenshot', async ({ page }) => {
        await openInventoryPage(page);
        await expectPageToLookLikeBaseline(page, '/user/inventory', 'user-inventory-page');
    });

    test('looks like the baseline-screenshot when objects are expanded', async ({ page }) => {
        await openInventoryPage(page);
        await openPageInReproducibleState(page, '/user/inventory');
        await expand(page, 'Headquarter');
        await expand(page, 'Building A');
        await expand(page, 'Room 101');
        await expand(page, 'Euro-box');
        await page.getByText('Drill', { exact: true }).waitFor();
        /*
         * Clicking a toggle gives it the focus, and whether a clicked button keeps the focus differs between the
         * browsers. Removing the focus and moving the mouse away makes the state of the page the same everywhere.
         */
        await page.evaluate(() => (document.activeElement as HTMLElement | null)?.blur());
        await page.mouse.move(0, 0);
        await expectCurrentPageToLookLikeBaseline(page, 'user-inventory-page-expanded');
    });
});
