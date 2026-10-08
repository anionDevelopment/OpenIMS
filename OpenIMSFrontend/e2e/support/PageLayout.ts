import { ElementHandle, Page, test } from '@playwright/test';
import { mkdirSync, writeFileSync } from 'fs';
import { dirname, join } from 'path';

/*
 * Writes down where every element of the current page is located and how big it is.
 *
 * Reason: a baseline-screenshot only ever gets compared with the screenshot of the same browser, so a page which
 * looks different in one browser than in the others stays unnoticed forever. Comparing the screenshots of the
 * browsers with each other instead does not work: the engines rasterize text differently, so thousands of pixels
 * differ on a page which looks the same. The geometry however does not depend on the rasterization, so it can be
 * compared directly. What is checked afterwards (by "check_layouts_are_similar" of ScriptCollection, which is
 * called by "Other/QualityCheck/RunTestcases.py") is that all browsers put every element at the same place.
 *
 * The files are written into the artifacts-folder and not into the resources-folder, because they are not a
 * baseline which somebody maintains: they only exist for the comparison inside the run which created them.
 */
export async function saveLayoutOfPage(page: Page, name: string, selectorsOfElementsToExclude: string[] = []): Promise<void> {
    /*
     * The elements which are masked in the screenshot are excluded here as well, together with everything they
     * contain. They are masked because their content is not reproducible, and a part of a page whose content is
     * not reproducible does not have a reproducible geometry either (the video-player of a camera for example
     * builds a different document depending on the state it is in).
     *
     * They are looked up by playwright and are handed over to the page afterwards, because a selector may use
     * one of the additions of playwright (":has-text(...)" for example), which the selector-engine of a browser
     * does not know and rejects.
     */
    const excludedElements: ElementHandle<SVGElement | HTMLElement>[] = [];
    for (const selector of selectorsOfElementsToExclude) {
        excludedElements.push(...await page.locator(selector).elementHandles());
    }
    const elements: ElementGeometry[] = await page.evaluate((excludedElements: Element[]) => {
        /*
         * Identifies an element by its position in the document-tree. That position is used and not for example
         * a css-selector of the element itself, because it exists for every element, is unique, and is the same
         * in every browser (the document is built from the same html everywhere).
         */
        function getPathOfElement(element: Element): string {
            const parts: string[] = [];
            let current: Element | null = element;
            while (current !== null && current.parentElement !== null) {
                const positionAmongSiblings: number = Array.from(current.parentElement.children).indexOf(current) + 1;
                parts.unshift(`${current.tagName.toLowerCase()}:nth-child(${positionAmongSiblings})`);
                current = current.parentElement;
            }
            return parts.join('>');
        }
        return Array.from(document.querySelectorAll('*'))
            .filter((element) => !excludedElements.some((excludedElement) => excludedElement.contains(element)))
            .map((element) => ({ element: element, rectangle: element.getBoundingClientRect() }))
            /* Elements which are not rendered at all have no geometry which could be compared. */
            .filter((entry) => entry.rectangle.width !== 0 || entry.rectangle.height !== 0)
            .map((entry) => ({
                path: getPathOfElement(entry.element),
                /* The position is relative to the document and not to the viewport, so that it does not depend
                   on how far the page happened to be scrolled while the screenshot was taken. */
                x: Math.round(entry.rectangle.x + window.scrollX),
                y: Math.round(entry.rectangle.y + window.scrollY),
                width: Math.round(entry.rectangle.width),
                height: Math.round(entry.rectangle.height)
            }));
    }, excludedElements);
    for (const excludedElement of excludedElements) {
        await excludedElement.dispose();
    }
    const file: string = join(getFolderOfTheLayouts(), `${name}.json`);
    mkdirSync(dirname(file), { recursive: true });
    writeFileSync(file, JSON.stringify(elements, null, 4), { encoding: 'utf-8' });
}

interface ElementGeometry {
    path: string;
    x: number;
    y: number;
    width: number;
    height: number;
}

/*
 * The folder the layout of the current browser and platform is written to. It is derived from the output-folder
 * of playwright, so that a codeunit whose playwright-project is not located in the codeunit-folder itself does
 * not have to configure the folder a second time.
 */
function getFolderOfTheLayouts(): string {
    return join(test.info().project.outputDir, '..', 'VisualRegressionLayouts', test.info().project.name, process.platform);
}
