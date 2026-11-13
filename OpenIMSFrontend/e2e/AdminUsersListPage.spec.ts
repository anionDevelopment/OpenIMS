import { test } from '@playwright/test';
import { expectPageToLookLikeBaseline } from './support/VisualRegression';
import { simulateLoggedInUser } from './support/AuthenticatedSession';
import { respondWith } from './support/SimulatedBackend';

/*
 * The user-overview of the admin-area. It displays data which the backend delivers, so that data is answered
 * with fixed values here: otherwise the screenshot would depend on the content of a database.
 */
const rolesOfTheSimulatedInstallation: string[] = ['Administrators', 'Users'];

const usersOfTheSimulatedInstallation = [
    { id: '00000000-0000-0000-0000-000000000001', name: 'Testuser', roles: ['Administrators', 'Users'] },
    { id: '00000000-0000-0000-0000-000000000002', name: 'user01', roles: ['Users'] }
];

test.describe('Admin-users-list-page', () => {
    test('looks like the baseline-screenshot', async ({ page }) => {
        await simulateLoggedInUser(page);
        await respondWith(page, '**/API/v1/UserController/GetAllRoles*', rolesOfTheSimulatedInstallation);
        await respondWith(page, '**/API/v1/UserController/GetAllUsers*', usersOfTheSimulatedInstallation);
        await expectPageToLookLikeBaseline(page, '/admin/users', 'admin-users-list-page');
    });
});
