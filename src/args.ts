import { ArgumentParser } from 'argparse';

const parser = new ArgumentParser({
    addHelp: true,
    description: 'みんなの食堂注文くん',
});

parser.addArgument(['--email'], {
    type: 'string',
    help: 'login email',
});

parser.addArgument(['--password'], {
    type: 'string',
    help: 'login password',
});

parser.addArgument(['--dist'], {
    type: 'string',
    defaultValue: null,
    dest: 'verbose',
    required: false,
});

parser.addArgument(['-v'], {
    action: 'storeTrue',
    defaultValue: false,
    dest: 'verbose',
});

export interface ArgTypes {
    email: string;
    password: string;
    verbose: boolean;
    dist: string | null;
}

export const args: ArgTypes = parser.parseArgs();
