import { test } from '@playwright/test';
import { expectPageToLookLikeBaseline } from './support/VisualRegression';

test.describe('Home-page', () => {
    test('looks like the baseline-screenshot', async ({ page }) => {
        await expectPageToLookLikeBaseline(page, '/', 'home-page');
    });
});
