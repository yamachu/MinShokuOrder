import { launch } from 'puppeteer';
import { args, ArgTypes } from './args';
import {
    LIST_ORDER_BUTTON,
    LOGIN_BUTTON,
    LOGIN_EMAIL,
    LOGIN_PASSWORD,
    LIST_DATES,
    LIST_MENU_NAMES,
    LIST_ORDER_BUTTONS,
    DETAIL_ORDER_BUTTON,
    LIST_ORDER_DEADLINE_BUTTON,
} from './query';
import { LOGIN_URL, MENU_LIST_URL, CONFIRM_URL, ORDER_SUCCESS_URL } from './url';
import { _dumpInfo } from './utils';

const main = async (args: ArgTypes) => {
    const dumpInfo = _dumpInfo(args.verbose);
    const browser = await launch({
        args: ['lang=ja,en-US,en'],
    });
    const page = await browser.newPage();
    await page.setViewport({ width: 1200, height: 800 });

    await page.goto(LOGIN_URL, { waitUntil: 'networkidle0' });
    await page.type(LOGIN_EMAIL, args.email);
    await page.type(LOGIN_PASSWORD, args.password);
    await page.click(LOGIN_BUTTON);
    await page.waitForNavigation({ waitUntil: 'networkidle0' });

    await page.goto(MENU_LIST_URL, { waitUntil: 'networkidle0' });

    const notCancelableButtonExist = (await page.$x(LIST_ORDER_DEADLINE_BUTTON)).length !== 0;
    const dates = await Promise.all(
        (await page.$x(LIST_DATES))
            .map(async (v) => await page.evaluate((elm) => elm.innerText, v))
            .map(async (v) => (await v).trim())
    );
    const menus = await Promise.all(
        (await page.$x(LIST_MENU_NAMES)).map(
            async (v) => await page.evaluate((elm) => elm.innerText, v)
        )
    );
    const ordered = await Promise.all(
        (await page.$x(LIST_ORDER_BUTTONS))
            .map(async (v) => await page.evaluate((elm) => elm.className, v))
            .map(async (v) => (await v).includes('menu__btn--cancel'))
    );

    const menuDetails = ordered.map((v, i) => ({
        date: dates[i + (notCancelableButtonExist ? 1 : 0)],
        menu: menus[i + (notCancelableButtonExist ? 1 : 0)],
        ordered: v,
    }));

    dumpInfo(menuDetails);

    const willOrderIndex = ordered
        .map((v, i) => [v, i])
        .filter((v) => !v[0])
        .map((v) => v[1] as number);

    for (let i of willOrderIndex) {
        await Promise.all([
            (await page.$x(LIST_ORDER_BUTTON`${i + 1}`))[0].click(),
            page.waitForResponse(CONFIRM_URL),
        ]);

        await page.waitForXPath(DETAIL_ORDER_BUTTON);

        await Promise.all([
            (await page.$x(DETAIL_ORDER_BUTTON))[0].click(),
            page.waitForResponse((res) => res.url().indexOf(ORDER_SUCCESS_URL) !== -1),
        ]);

        if (args.dist !== null)
            await page.screenshot({
                path: `${args.dist}/order_${dates[i].split('(')[0].replace('/', '-')}_${
                    menus[i]
                }.png`,
            });

        await page.goto(MENU_LIST_URL, { waitUntil: 'networkidle0' });
    }

    await browser.close();
};

main(args);
