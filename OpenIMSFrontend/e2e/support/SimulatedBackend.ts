import { Page } from '@playwright/test';

/*
 * Answers all requests which match the given url-pattern with the given content.
 *
 * The visual-regression-tests run without a backend, so every request which the application sends
 * while a page is rendered has to be answered by Playwright itself. Doing it this way also keeps
 * the screenshots reproducible, because they do not depend on the content of a database.
 *
 * This function has to be called before the page is opened.
 */
export async function respondWith(page: Page, urlPattern: string, content: unknown): Promise<void> {
    await page.route(urlPattern, (route) => route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(content)
    }));
}
