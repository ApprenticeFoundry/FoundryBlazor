/*
 * Adapted from Scott Harden's EXCELLENT blog post,
 * "Draw Animated Graphics in the Browser with Blazor WebAssembly"
 * https://swharden.com/blog/2021-01-07-blazor-canvas-animated-graphics/
 */

/*This is called from the Blazor component's Initialize method*/

function initSVCRenderJS(instance) {
  // instance is the Blazor component dotnet reference
  window.theInstance3 = instance;

  // tell the window we want to handle the resize event
  window.addEventListener("resize", WindowResized);

  window.addEventListener("keydown", keyDown);
  window.addEventListener("keyup", keyUp);
  window.addEventListener("keypress", keyPress);

  const svg = getSVGNode();

  if (svg) {
    svg.addEventListener("wheel", wheelChange);
    svg.addEventListener("mousedown", mouseDown);
    svg.addEventListener("mouseup", mouseUp);
    svg.addEventListener("mousemove", mouseMove);
    svg.addEventListener("mouseout", mouseOut);
    svg.addEventListener("mouseenter", mouseEnter);
    // canvas.addEventListener('mousein', mouseIn, false);

    //https://www.geeksforgeeks.org/html-dom-ondragover-event/
    // canvas.addEventListener("dragstart", onDragStart);
    // canvas.addEventListener("drag", onDrag);
    // canvas.addEventListener("dragend", onDragEnd);
    // canvas.addEventListener("dragenter", onDragEnter);
    // canvas.addEventListener("dragover", onDragOver);
    // canvas.addEventListener("dragleave", onDragLeave);
    // canvas.addEventListener("drop", onDrop);
  }

  // Call resize now
  //SRS Fix in the future
  WindowResized();

  // request an animation frame, telling window to call renderJS
  // https://developer.mozilla.org/en-US/docs/Web/API/window/requestAnimationFrame
  window.requestAnimationFrame(renderJS);
}

function getSVGNode() {
    var holder = document.getElementById("svgHolder");
    // find the canvas within the renderfragment
    var svg = holder.querySelector('SVG');
    return svg;
}




/*This is called whenever we have requested an animation frame*/
function renderJS(timeStamp) {
    // Call the blazor component's [JSInvokable] RenderInBlazor method
    theInstance3.invokeMethodAsync('RenderInBlazor');
    // request another animation frame
    window.requestAnimationFrame(renderJS);
}

/*This is called whenever the browser (and therefore the canvas) is resized*/
function WindowResized() {
    // canvasHolder is the ID of the div that wraps the renderfragment
    // content(Canvas) in the blazor app
    // find the canvas within the renderfragment

    // Call the blazor component's [JSInvokable] ResizeInBlazor method
    theInstance3.invokeMethodAsync('ResizeInBlazor', window.innerWidth, window.innerHeight);
}

//Handle the canvas.wheel event
// https://developer.mozilla.org/en-US/docs/Web/API/Element/wheel_event
function wheelChange(e) {
    e.preventDefault();
    var args = canvasWheelChangeArgs(e);
    theInstance3.invokeMethodAsync('OnWheelChange', args);
}

//Handle the canvas.mouseout event
function mouseOut(e) {
    e.preventDefault();
    var args = canvasMouseMoveArgs(e);
    theInstance3.invokeMethodAsync('OnMouseOut', args);
}

//Handle the canvas.mouseout event
function mouseIn(e) {
    e.preventDefault();
    var args = canvasMouseMoveArgs(e);
    theInstance3.invokeMethodAsync('OnMouseIn', args);
}

function mouseEnter(e) {
    e.preventDefault();
    var args = canvasMouseMoveArgs(e);
    theInstance3.invokeMethodAsync('OnMouseEnter', args);
}

//Handle the window.mousedown event
function mouseDown(e) {
    e.preventDefault();
    var args = canvasMouseMoveArgs(e);
    theInstance3.invokeMethodAsync('OnMouseDown', args);
}

//Handle the window.mouseup event
function mouseUp(e) {
    e.preventDefault();
    var args = canvasMouseMoveArgs(e);
    theInstance3.invokeMethodAsync('OnMouseUp', args);
}

//Handle the window.mousemove event
function mouseMove(e) {
    e.preventDefault();
    var args = canvasMouseMoveArgs(e);
    theInstance3.invokeMethodAsync('OnMouseMove', args);
}

//Handle the canvas.keydown event
function keyDown(e) {
    // NOTE: prevent default will disable ability for HTML input fields to accept key events
    // e.preventDefault();
    var args = canvasKeyboardEventArgs(e);
    theInstance3.invokeMethodAsync('OnKeyDown', args);
}

//Handle the canvas.keyup event
function keyUp(e) {
    // NOTE: prevent default will disable ability for HTML input fields to accept key events
    // e.preventDefault();
    var args = canvasKeyboardEventArgs(e);
    theInstance3.invokeMethodAsync('OnKeyUp', args);
}

//Handle the canvas.keypress event
function keyPress(e) {
    // NOTE: prevent default will disable ability for HTML input fields to accept key events
    // e.preventDefault();
    var args = canvasKeyboardEventArgs(e);
    theInstance3.invokeMethodAsync('OnKeyPress', args);
}

// function onDragStart(e) {
//     e.preventDefault();
//     var args = canvasDragArgs(e);
//     theInstance.invokeMethodAsync('OnDragStart', args);
// }

// function onDrag(e) {
//     e.preventDefault();
//     var args = canvasDragArgs(e);
//     theInstance.invokeMethodAsync('OnDrag', args);
// }

// function onDragEnd(e) {
//     e.preventDefault();
//     var args = canvasDragArgs(e);
//     theInstance.invokeMethodAsync('OnDragEnd', args);
// }
// function onDragEnter(e) {
//     e.preventDefault();
//     var args = canvasDragArgs(e);
//     theInstance.invokeMethodAsync('OnDragOver', args);
// }
// function onDragOver(e) {
//     e.preventDefault();
//     var args = canvasDragArgs(e);
//     theInstance.invokeMethodAsync('OnDragOver', args);
// }
// function onDragLeave(e) {
//     e.preventDefault();
//     var args = canvasDragArgs(e);
//     theInstance.invokeMethodAsync('OnDragLeave', args);
// }
// function onDrop(e) {
//     e.preventDefault();
//     var args = canvasDragArgs(e);
//     theInstance.invokeMethodAsync('OnDrop', args);
// }
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

//https://developer.mozilla.org/en-US/docs/Web/API/DataTransfer
// function dataTransferArgs(e) {
//     //var data = e.getData();
//     //console.log(data, "dataTransferArgs")

//     console.log(e, 'args');
//     if (e.files != null) console.log(e.files[0], 'files[0]');

//     if (e.items != null) console.log(e.items[0], 'items[0]');

//     return {
//         dropEffect: e.dropEffect,
//         effectAllowed: e.effectAllowed,
//         files: e.files,
//         items: e.items,
//         types: e.types,
//     };
// }

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



function showFileInput() {
    const fileInputContainer = getFileInputContainer();
    if (fileInputContainer) {
        fileInputContainer.style.zIndex = 20;
    } else {
        showFileInputNodeNotFound();
    }
}

function hideFileInput() {
    // We *must* hide file input after file is dropped over chrome
    const fileInputContainer = getFileInputContainer();
    if (fileInputContainer) {
        fileInputContainer.style.zIndex = 0;
    }
}

function saveAsFile(filename, bytesBase64) {
    var link = document.createElement('a');
    link.download = filename;
    link.href = 'data:application/octet-stream;base64,' + bytesBase64;
    document.body.appendChild(link); // Needed for Firefox
    link.click();
    document.body.removeChild(link);
}

function canvasPNGBase64() {
    return getCanvasNode().toDataURL();
}

function removeKeyEventListeners() {
    window.removeEventListener('keydown', keyDown);
    window.removeEventListener('keyup', keyUp);
    window.removeEventListener('keypress', keyPress);
}

function addKeyEventListeners() {
    window.addEventListener('keydown', keyDown);
    window.addEventListener('keyup', keyUp);
    window.addEventListener('keypress', keyPress);
}

window.CanvasFileInput = {
    ShowFileInput: showFileInput,
    HideFileInput: hideFileInput,
};

window.keyEventListeners = { remove: removeKeyEventListeners, add: addKeyEventListeners };
window.canvasPNGBase64 = canvasPNGBase64;
window.saveAsFile = saveAsFile;

window.initSCVRenderJS = initSVCRenderJS;
