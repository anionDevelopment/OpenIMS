import { defineConfig } from '@playwright/test';

/*
 * Configuration for the visual-regression-tests.
 *
 * The baseline-screenshots are stored per browser and per operating-system because the
 * rendering-result of a page depends on both. Baselines for a new combination have to be
 * generated once by running the update-command (see the ReadMe.md of this codeunit).
 */
export const applicationPort = 4200;
export const applicationUrl = `http://localhost:${applicationPort}`;

/*
 * Amount of pixels a screenshot is allowed to differ from its baseline.
 * The value is a compromise: it is small enough to detect real layout- and color-changes
 * (which usually affect several thousand pixels) but large enough to tolerate the
 * antialiasing-noise which occurs when the same page is rendered twice.
 * With the viewport defined below one screenshot has at least 1280*720=921600 pixels,
 * so 250 pixels are about 0.03 percent of the image.
 */
export const maximalAmountOfDifferentPixels = 250;

export default defineConfig({
    testDir: './e2e',
    outputDir: './Other/Artifacts/VisualRegressionTestResults',
    snapshotPathTemplate: './Other/Resources/VisualRegressionBaselines/{projectName}/{platform}/{arg}{ext}',
    fullyParallel: true,
    retries: 0,
    reporter: [
        ['list'],
        ['html', { outputFolder: './Other/Artifacts/VisualRegressionTestReport', open: 'never' }]
    ],
    expect: {
        toHaveScreenshot: {
            maxDiffPixels: maximalAmountOfDifferentPixels
        }
    },
    use: {
        baseURL: applicationUrl,
        /* A fixed viewport, scale-factor, color-scheme and timezone are required to get reproducible screenshots. */
        viewport: { width: 1280, height: 720 },
        deviceScaleFactor: 1,
        colorScheme: 'light',
        timezoneId: 'UTC',
        locale: 'en-US'
    },
    /* Every testcase is executed once per browser defined here. */
    projects: [
        { name: 'chromium', use: { browserName: 'chromium' } },
        { name: 'firefox', use: { browserName: 'firefox' } },
        { name: 'webkit', use: { browserName: 'webkit' } }
    ],
    webServer: {
        command: 'npm run start',
        url: applicationUrl,
        /* Building and serving the application takes a while, especially on the first run. */
        timeout: 600000,
        reuseExistingServer: true
    }
});
