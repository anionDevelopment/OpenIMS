import { test } from '@playwright/test';
import { expectPageToLookLikeBaseline } from './support/VisualRegression';

test.describe('Hint-not-found-page', () => {
    test('looks like the baseline-screenshot', async ({ page }) => {
        await expectPageToLookLikeBaseline(page, '/this-route-does-not-exist', 'hint-not-found-page');
    });
});
