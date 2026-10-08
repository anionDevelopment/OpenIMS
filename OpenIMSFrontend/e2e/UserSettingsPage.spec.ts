import { test } from '@playwright/test';
import { expectPageToLookLikeBaseline } from './support/VisualRegression';
import { simulateLoggedInUser } from './support/AuthenticatedSession';

test.describe('User-settings-page', () => {
    /*
     * The page is checked in both color-schemes, because the dark one is not a variation of a few colors of the
     * light one: every color-token of the theme has its own value there. A regression which only shows up in the
     * dark scheme - an element which keeps a bright background and becomes unreadable, for example - would stay
     * unnoticed if only one of the two was checked.
     */
    test('looks like the baseline-screenshot in the light color-scheme', async ({ page }) => {
        await simulateLoggedInUser(page, 'light');
        await expectPageToLookLikeBaseline(page, '/user/settings', 'user-settings-page-light');
    });

    test('looks like the baseline-screenshot in the dark color-scheme', async ({ page }) => {
        await simulateLoggedInUser(page, 'dark');
        await expectPageToLookLikeBaseline(page, '/user/settings', 'user-settings-page-dark');
    });
});
