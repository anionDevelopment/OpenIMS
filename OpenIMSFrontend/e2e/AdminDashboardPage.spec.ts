import { test } from '@playwright/test';
import { expectPageToLookLikeBaseline } from './support/VisualRegression';
import { simulateLoggedInUser } from './support/AuthenticatedSession';

test.describe('Admin-dashboard-page', () => {
    /* The simulated user is an administrator, so the admin-area is reachable. */
    test('looks like the baseline-screenshot', async ({ page }) => {
        await simulateLoggedInUser(page);
        await expectPageToLookLikeBaseline(page, '/admin/dashboard', 'admin-dashboard-page');
    });
});
