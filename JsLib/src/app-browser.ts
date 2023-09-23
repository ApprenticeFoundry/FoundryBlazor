import { App } from './app';

export class AppBrowser extends App {
    public CopyText(text: string) {
        navigator.clipboard.writeText(text).then(
            () => {
                const message = `Successfully Copied ${text}`;
                console.log(`CopyText ${message}`);
                this.dotNetObjectReference.invokeMethodAsync('OnCopySuccess', message);
            },
            () => {
                const message = `Error: Could not copy ${text}`;
                console.error(`CopyText ${message}`);
                this.dotNetObjectReference.invokeMethodAsync('OnCopyError', message);
            }
        );
    }
    public BoundingClientRect(elementId: string) {
        const node = document.getElementById(elementId);
        if (Boolean(node)) {
            const boundingBox = node.getBoundingClientRect();
            this.dotNetObjectReference.invokeMethodAsync('OnBoundingClientRect', boundingBox);
        } else {
            this.dotNetObjectReference.invokeMethodAsync('OnBoundingClientRect', null);
        }
    }
    public HTMLWindow(): { InnerWidth: number; InnerHeight: number } {
        return { InnerWidth: window.innerWidth, InnerHeight: window.innerHeight };
    }
}
