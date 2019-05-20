import * as fs from 'fs-extra';

export const mkdir = async (path: string) => {
    return new Promise((resolve, reject) => {
        fs.mkdirs(path, (err) => {
            err ? reject(err) : resolve();
        });
    });
};

export const _dumpInfo = (show: boolean) => (...val: any[]) => {
    if (show) console.info(...val);
};
