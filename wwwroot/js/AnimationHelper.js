/*
 * Adapted from Scott Harden's EXCELLENT blog post,
 * "Draw Animated Graphics in the Browser with Blazor WebAssembly"
 * https://swharden.com/blog/2021-01-07-blazor-canvas-animated-graphics/
 */

/*This is called from the Blazor component's Initialize method*/

function initAnimationJS(instance) {
    // instance is the Blazor component dotnet reference
    window.theInstance2 = instance;



    // request an animation frame, telling window to call renderJS
    // https://developer.mozilla.org/en-US/docs/Web/API/window/requestAnimationFrame
    window.requestAnimationFrame(renderAnimationJS);
}


/*This is called whenever we have requested an animation frame*/
function renderAnimationJS(timeStamp) {
  // Call the blazor component's [JSInvokable] RenderInBlazor method
  theInstance2.invokeMethodAsync("RenderAnimation");
  // request another animation frame
  window.requestAnimationFrame(renderAnimationJS);
}

/*This is called whenever the browser (and therefore the canvas) is resized*/
function WindowResized() {
    // canvasHolder is the ID of the div that wraps the renderfragment
    // content(Canvas) in the blazor app
    // find the canvas within the renderfragment

    // Call the blazor component's [JSInvokable] ResizeInBlazor method
    theInstance2.invokeMethodAsync('ResizeAnimation', window.innerWidth, window.innerHeight);
}



window.initAnimationJS = initAnimationJS;
