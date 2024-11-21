/*
 * Adapted from Scott Harden's EXCELLENT blog post,
 * "Draw Animated Graphics in the Browser with Blazor WebAssembly"
 * https://swharden.com/blog/2021-01-07-blazor-canvas-animated-graphics/
 */

/*This is called from the Blazor component's Initialize method*/
//const fileInputId = 'fileInputHolder';

function initJSIntegration(instance) {
    //no need to do anything if we have already set up the callback
    if (window.DotNetCallBack) return;

    // instance is the Blazor component dotnet reference
    window.DotNetCallBack = instance;

    // tell the window we want to handle the resize event
    window.addEventListener('resize', WindowResized);

    window.addEventListener('keydown', keyDown);
    window.addEventListener('keyup', keyUp);
    window.addEventListener('keypress', keyPress);

    // Call resize now
    //SRS Fix in the future
    WindowResized();

    // request an animation frame, telling window to call renderJS
    // https://developer.mozilla.org/en-US/docs/Web/API/window/requestAnimationFrame

    StartAnimation();
}

window.initJSIntegration = initJSIntegration;


function destroyJSIntegration() {
  //no need to do anything if we have already set up the callback
  if (window.DotNetCallBack == null) return;

  // instance is the Blazor component dotnet reference
  window.DotNetCallBack = null;

  // tell the window we want to handle the resize event
  window.removeEventListener("resize", WindowResized);

  window.removeEventListener("keydown", keyDown);
  window.removeEventListener("keyup", keyUp);
  window.removeEventListener("keypress", keyPress);


  // request an animation frame, telling window to call renderJS
  // https://developer.mozilla.org/en-US/docs/Web/API/window/requestAnimationFrame

    StopAnimation();
}

window.destroyJSIntegration = destroyJSIntegration;


window.AnimationRequest = null;

/*This is called whenever we have requested an animation frame*/
function renderJS(timeStamp) {
    // Call the blazor component's [JSInvokable] RenderInBlazor method
    DotNetCallBack.invokeMethodAsync('RenderFrameEventCalled');
    // request another animation frame
    window.requestAnimationFrame(renderJS);
}

//https://stackoverflow.com/questions/10735922/how-to-stop-a-requestanimationframe-recursion-loop
function StartAnimation() {
    if (window.AnimationRequest == null) 
        window.AnimationRequest = window.requestAnimationFrame(renderJS);
}

function StopAnimation() {
    if (window.AnimationRequest != null) 
        window.cancelAnimationFrame(window.AnimationRequest);
    window.AnimationRequest = null;
}

window.StartAnimation = StartAnimation;
window.StopAnimation = StopAnimation;

function CaptureCanvasEvents() {
    const canvas = getCanvasNode();

    if (canvas) {
        canvas.addEventListener('wheel', wheelChange);
        canvas.addEventListener('mousedown', mouseDown);
        canvas.addEventListener('mouseup', mouseUp);
        canvas.addEventListener('mousemove', mouseMove);
        //canvas.addEventListener("mouseout", mouseOut);
        //canvas.addEventListener("mouseenter", mouseEnter);
    }
}

function RemoveCanvasEvents() {
    const canvas = getCanvasNode();

    if (canvas) {
        canvas.removeEventListener('wheel', wheelChange);
        canvas.removeEventListener('mousedown', mouseDown);
        canvas.removeEventListener('mouseup', mouseUp);
        canvas.removeEventListener('mousemove', mouseMove);
        //canvas.removeEventListener("mouseout", mouseOut);
        //canvas.removeEventListener("mouseenter", mouseEnter);
    }
}

window.CaptureMouseEventsForCanvas = CaptureCanvasEvents;
window.RemoveMouseEventsForCanvas = RemoveCanvasEvents;

function CaptureSVGEvents() {
    const canvas = getSVGNode();

    if (canvas) {
        canvas.addEventListener('wheel', wheelChange);
        canvas.addEventListener('mousedown', mouseDown);
        canvas.addEventListener('mouseup', mouseUp);
        canvas.addEventListener('mousemove', mouseMove);
        //canvas.addEventListener("mouseout", mouseOut);
        //canvas.addEventListener("mouseenter", mouseEnter);
    }
}

function RemoveSVGEvents() {
    const canvas = getSVGNode();

    if (canvas) {
        canvas.removeEventListener('wheel', wheelChange);
        canvas.removeEventListener('mousedown', mouseDown);
        canvas.removeEventListener('mouseup', mouseUp);
        canvas.removeEventListener('mousemove', mouseMove);
        //canvas.removeEventListener("mouseout", mouseOut);
        //canvas.removeEventListener("mouseenter", mouseEnter);
    }
}
window.CaptureMouseEventsForSVG = CaptureSVGEvents;
window.RemoveMouseEventsForSVG = RemoveSVGEvents;

function getCanvasNode() {
    var holder = document.getElementById('canvasHolder');
    // find the canvas within the renderfragment
    var canvas = holder.querySelector('canvas');
    return canvas;
}

function getSVGNode() {
    var holder = document.getElementById('svgHolder');
    // find the canvas within the renderfragment
    var svg = holder.querySelector('SVG');
    return svg;
}

// function getFileInputContainer() {
//     var node = document.getElementById(fileInputId);
//     return node;
// }

/*This is called whenever the browser (and therefore the canvas) is resized*/
function WindowResized() {
    // canvasHolder is the ID of the div that wraps the renderfragment
    // content(Canvas) in the blazor app
    // find the canvas within the renderfragment

    // Call the blazor component's [JSInvokable] ResizeInBlazor method
    DotNetCallBack.invokeMethodAsync('ResizeWindowEventCalled', window.innerWidth, window.innerHeight);
}

//Handle the canvas.wheel event
// https://developer.mozilla.org/en-US/docs/Web/API/Element/wheel_event
function wheelChange(e) {
    e.preventDefault();
    var args = canvasWheelChangeArgs(e);
    DotNetCallBack.invokeMethodAsync('OnWheelChange', args);
}

//Handle the canvas.mouseout event
function mouseOut(e) {
    e.preventDefault();
    var args = canvasMouseMoveArgs(e);
    DotNetCallBack.invokeMethodAsync('OnMouseOut', args);
}

//Handle the canvas.mouseout event
function mouseIn(e) {
    e.preventDefault();
    var args = canvasMouseMoveArgs(e);
    DotNetCallBack.invokeMethodAsync('OnMouseIn', args);
}

function mouseEnter(e) {
    e.preventDefault();
    var args = canvasMouseMoveArgs(e);
    DotNetCallBack.invokeMethodAsync('OnMouseEnter', args);
}

//Handle the window.mousedown event
function mouseDown(e) {
    e.preventDefault();
    var args = canvasMouseMoveArgs(e);
    DotNetCallBack.invokeMethodAsync('OnMouseDown', args);
}

//Handle the window.mouseup event
function mouseUp(e) {
    e.preventDefault();
    var args = canvasMouseMoveArgs(e);
    DotNetCallBack.invokeMethodAsync('OnMouseUp', args);
}

//Handle the window.mousemove event
function mouseMove(e) {
    e.preventDefault();
    var args = canvasMouseMoveArgs(e);
    DotNetCallBack.invokeMethodAsync('OnMouseMove', args);
}

//Handle the canvas.keydown event
function keyDown(e) {
    // NOTE: prevent default will disable ability for HTML input fields to accept key events
    // e.preventDefault();
    var args = canvasKeyboardEventArgs(e);
    DotNetCallBack.invokeMethodAsync('OnKeyDown', args);
}

//Handle the canvas.keyup event
function keyUp(e) {
    // NOTE: prevent default will disable ability for HTML input fields to accept key events
    // e.preventDefault();
    var args = canvasKeyboardEventArgs(e);
    DotNetCallBack.invokeMethodAsync('OnKeyUp', args);
}

//Handle the canvas.keypress event
function keyPress(e) {
    // NOTE: prevent default will disable ability for HTML input fields to accept key events
    // e.preventDefault();
    var args = canvasKeyboardEventArgs(e);
    DotNetCallBack.invokeMethodAsync('OnKeyPress', args);
}

// Extend the CanvasMouseArgs.cs class (and this) as necessary
function canvasMouseMoveArgs(e) {
    return {
        ScreenX: e.screenX,
        ScreenY: e.screenY,
        ClientX: e.clientX,
        ClientY: e.clientY,
        MovementX: e.movementX,
        MovementY: e.movementY,
        OffsetX: e.offsetX,
        OffsetY: e.offsetY,
        AltKey: e.altKey,
        CtrlKey: e.ctrlKey,
        MetaKey: e.metaKey,
        ShiftKey: e.shiftKey,
        Button: e.button,
        Buttons: e.button,
        Bubbles: e.bubbles,
    };
}

function canvasDragArgs(e) {
    return {
        // DataTransfer: dataTransferArgs(e.dataTransfer),
        ScreenX: e.screenX,
        ScreenY: e.screenY,
        ClientX: e.clientX,
        ClientY: e.clientY,
        MovementX: e.movementX,
        MovementY: e.movementY,
        OffsetX: e.offsetX,
        OffsetY: e.offsetY,
        AltKey: e.altKey,
        CtrlKey: e.ctrlKey,
        MetaKey: e.metaKey,
        ShiftKey: e.shiftKey,
        Button: e.button,
        Buttons: e.button,
        Bubbles: e.bubbles,
    };
}

function canvasKeyboardEventArgs(e) {
    return {
        AltKey: e.altKey,
        CtrlKey: e.ctrlKey,
        MetaKey: e.metaKey,
        ShiftKey: e.shiftKey,
        Bubbles: e.bubbles,
        Code: e.code,
        IsComposing: e.isComposing,
        Key: e.key,
        Repeat: e.repeat,
        Location: e.location,
    };
}

function canvasWheelChangeArgs(e) {
    return {
        DeltaX: e.deltaX,
        DeltaY: e.deltaY,
        DeltaZ: e.deltaZ,
        DeltaMode: e.deltaMode,
    };
}

// function showFileInputNodeNotFound() {
//     window.alert(
//         `No file input node with id='${fileInputId}' was found in CanvasComponent.  You will not be able to drop files.`
//     );
// }

// function showFileInput() {
//     const fileInputContainer = getFileInputContainer();
//     if (fileInputContainer) {
//         fileInputContainer.style.zIndex = 20;
//     } else {
//         showFileInputNodeNotFound();
//     }
// }

// function hideFileInput() {
//     // We *must* hide file input after file is dropped over chrome
//     const fileInputContainer = getFileInputContainer();
//     if (fileInputContainer) {
//         fileInputContainer.style.zIndex = 0;
//     }
// }

// function saveAsFile(filename, bytesBase64) {
//     var link = document.createElement('a');
//     link.download = filename;
//     link.href = 'data:application/octet-stream;base64,' + bytesBase64;
//     document.body.appendChild(link); // Needed for Firefox
//     link.click();
//     document.body.removeChild(link);
// }

// function canvasPNGBase64() {
//     return getCanvasNode().toDataURL();
// }

// function removeKeyEventListeners() {
//     window.removeEventListener('keydown', keyDown);
//     window.removeEventListener('keyup', keyUp);
//     window.removeEventListener('keypress', keyPress);
// }

// function addKeyEventListeners() {
//     window.addEventListener('keydown', keyDown);
//     window.addEventListener('keyup', keyUp);
//     window.addEventListener('keypress', keyPress);
// }

// window.CanvasFileInput = {
//     ShowFileInput: showFileInput,
//     HideFileInput: hideFileInput,
// };

//window.keyEventListeners = { remove: removeKeyEventListeners, add: addKeyEventListeners };
//window.canvasPNGBase64 = canvasPNGBase64;
//window.saveAsFile = saveAsFile;

class VideoManager {
    id = 'DEFAULT_ID';

    _showAlert() {
        const msg = `No video node with id=${this.id} is available.`;
        window.alert(msg);
    }

    play(id) {
        this.id = id;
        const node = document.getElementById(id);
        if (node) {
            node.play();
        } else {
            this._showAlert();
        }
    }

    pause(id) {
        this.id = id;
        const node = document.getElementById(id);
        if (node) {
            node.pause();
        } else {
            this._showAlert();
        }
    }

    restart(id) {
        this.id = id;
        const node = document.getElementById(id);
        if (node) {
            node.pause();
            node.currentTime = 0;
            this.play(id);
        } else {
            this._showAlert();
        }
    }
}
window.VideoManager = new VideoManager();

class Browser {
    getWindowDimensions() {
        return {
            innerWidth: window.innerWidth,
            innerHeight: window.innerHeight,
        };
    }
    canvasPNGBase64(id = 'canvasHolder') {
        const containerNode = document.getElementById(id);
        let canvasNode = null;
        if (Boolean(containerNode)) canvasNode = containerNode.getElementsByTagName('canvas').item(0);

        if (Boolean(canvasNode)) {
            console.log('canvasNode=', canvasNode);
            return canvasNode.toDataURL();
        }
    }
    clickButton(id) {
        const button = document.getElementById(id);
        if (Boolean(button)) {
            button.click();
        }
    }
}
window.Browser = new Browser();
