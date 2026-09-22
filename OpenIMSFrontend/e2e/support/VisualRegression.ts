import { expect, Page } from '@playwright/test';
import { saveLayoutOfPage } from './PageLayout';

/*
 * Elements whose content changes without a change of the layout. They are masked in every
 * screenshot because otherwise every release would invalidate all baseline-screenshots.
 * (The footer for example contains the version of the application.)
 */
const selectorsOfVolatileElements: string[] = ['app-footer'];

/*
 * Opens the given route and compares the resulting screenshot with the baseline-screenshot
 * which belongs to the current browser and operating-system.
 * The comparison-tolerance is defined centrally in playwright.config.ts.
 */
export async function expectPageToLookLikeBaseline(page: Page, route: string, baselineName: string): Promise<void> {
    await openPageInReproducibleState(page, route);
    await expect(page).toHaveScreenshot(`${baselineName}.png`, {
        fullPage: true,
        caret: 'hide',
        animations: 'disabled',
        mask: selectorsOfVolatileElements.map((selector) => page.locator(selector))
    });
    /*
     * The screenshot above is only compared with the baseline of the same browser, which can not detect that a
     * page looks different in one browser than in the others. The geometry of the page is therefore written
     * down additionally; it is compared across the browsers after the run (see saveLayoutOfPage).
     */
    await saveLayoutOfPage(page, baselineName, selectorsOfVolatileElements);
}

/*
 * The font which is used on every page. A page which is rendered before this font is available looks completely
 * different, so a screenshot which is taken at that moment does not differ from its baseline by a few pixels but
 * by a large part of the image. The font is delivered with the application, so it is available immediately; the
 * check is kept nevertheless, because it makes a missing font fail with a clear message instead of with a
 * difference of the whole page, which would look like a regression of the user-interface.
 * Only this font is checked and not for example the icon-font, because a font which a page does not use is
 * not loaded at all and would therefore never be reported as available.
 */
const requiredFont = '16px Roboto';

/*
 * Navigates to the given route and waits until the page is rendered completely.
 * Without this waiting the screenshots would be flaky.
 */
async function openPageInReproducibleState(page: Page, route: string): Promise<void> {
    await page.goto(route, { waitUntil: 'networkidle' });
    await page.evaluate(() => document.fonts.ready);
    /*
     * Waiting for "document.fonts.ready" alone is not sufficient: it also resolves if the loading of a font
     * failed. Therefore it is checked explicitly that the required font is really usable now. If it is not
     * available then the testcase fails here with a clear message instead of reporting a difference of the
     * whole page, which would look like a regression of the user-interface.
     */
    await page.waitForFunction((font: string) => document.fonts.check(font), requiredFont);
}
