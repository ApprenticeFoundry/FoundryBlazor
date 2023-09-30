import { App } from './app';

export class AppBrowser extends App {
    public AnimationRequest: any = null;

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

    public Initialize(ref: any): void {
        if (this.dotNetObjectReference == null)
            this.SetDotNetObjectReference(ref);
    }

    public Finalize(): void {
        if (this.dotNetObjectReference != null)
        {
            this.StopAnimation();
            this.dotNetObjectReference.dispose();
            this.dotNetObjectReference = null;
        }
    }

    private RenderJS(self: any) {
        // Call the blazor component's [JSInvokable] RenderInBlazor method
        if ( self.dotNetObjectReference != null)
        {
            self.dotNetObjectReference.invokeMethodAsync('RenderFrameEventCalled');
            // request another animation frame
            self.AnimationRequest = window.requestAnimationFrame(() => self.RenderJS(self));
        }
    }
    public StartAnimation() {
        if (this.AnimationRequest == null)
            this.AnimationRequest = window.requestAnimationFrame(() => {
                this.RenderJS(this);
            });
    }
    public StopAnimation() {
        if (this.AnimationRequest != null) 
            window.cancelAnimationFrame(this.AnimationRequest);

        this.AnimationRequest = null;
    }
}
