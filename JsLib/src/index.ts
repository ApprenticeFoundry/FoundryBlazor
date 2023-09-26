import { AppBrowser } from './app-browser';

namespace JSInterop {
    export function Load(): void {
        window['AppBrowser'] = new AppBrowser();
    }

    JSInterop.Load();
}
