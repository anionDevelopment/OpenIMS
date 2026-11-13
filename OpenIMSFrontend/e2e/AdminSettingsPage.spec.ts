import { test } from '@playwright/test';
import { expectPageToLookLikeBaseline } from './support/VisualRegression';
import { simulateLoggedInUser } from './support/AuthenticatedSession';
import { respondWith } from './support/SimulatedBackend';

test.describe('Admin-settings-page', () => {
    /* The settings are answered with a fixed value, so the state of the toggle does not depend on a database. */
    test('looks like the baseline-screenshot', async ({ page }) => {
        await simulateLoggedInUser(page);
        await respondWith(page, '**/API/v1/OpenIMSBackend/GetGeneralSettings*', { registrationIsEnabled: true });
        await expectPageToLookLikeBaseline(page, '/admin/settings', 'admin-settings-page');
    });
});
