export const LOGIN_EMAIL = '#user_email';
export const LOGIN_PASSWORD = '#user_password';
export const LOGIN_BUTTON = '#new_user > div > ul > li > input';

export const LIST_DATES = '//*[@id="top"]/div/main/div/ul[2]/li[*]/section/h2/span';
export const LIST_MENU_NAMES =
    '//*[@id="top"]/div/main/div/ul[2]/li[*]/section/ul/li[1]/section/div/h3';
export const LIST_ORDER_BUTTONS =
    '//*[@id="top"]/div/main/div/ul[2]/li[*]/section/ul/li[1]/section/div/form/input[@name="commit"]';
export const LIST_ORDER_BUTTON = (_: any, ...args: any[]) =>
    `//*[@id="top"]/div/main/div/ul[2]/li[${
        args[0]
    }]/section/ul/li[1]/section/div/form/input[@name="commit"]`;

export const DETAIL_ORDER_BUTTON =
    '//*[@id="top"]/div/main/div[1]/section/form/input[@name="commit"]';
