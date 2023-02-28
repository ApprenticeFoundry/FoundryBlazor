/*
 * Adapted from Scott Harden's EXCELLENT blog post,
 * "Draw Animated Graphics in the Browser with Blazor WebAssembly"
 * https://swharden.com/blog/2021-01-07-blazor-canvas-animated-graphics/
 */

/*This is called from the Blazor component's Initialize method*/
const fileInputId = 'fileInputHolder';
function initRenderJS(instance) {
    // instance is the Blazor component dotnet reference
    window.theInstance = instance;

    // tell the window we want to handle the resize event
    window.addEventListener('resize', WindowResized);

    window.addEventListener('keydown', keyDown);
    window.addEventListener('keyup', keyUp);
    window.addEventListener('keypress', keyPress);

    const canvas = getCanvasNode();
    // const canvasContainer = getCanvasContainer();

    if (canvas) {
        canvas.addEventListener('wheel', wheelChange);
        canvas.addEventListener('mousedown', mouseDown);
        canvas.addEventListener('mouseup', mouseUp);
        canvas.addEventListener('mousemove', mouseMove);
        canvas.addEventListener('mouseout', mouseOut);
        canvas.addEventListener('mouseenter', mouseEnter);
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

function getCanvasNode() {
    var holder = document.getElementById('canvasHolder');
    // find the canvas within the renderfragment
    var canvas = holder.querySelector('canvas');
    return canvas;
}

// function getCanvasContainer() {
//     var node = document.getElementById('canvasHolder');
//     return node;
// }

function getFileInputContainer() {
    var node = document.getElementById(fileInputId);
    return node;
}

/*This is called whenever we have requested an animation frame*/
function renderJS(timeStamp) {
    // Call the blazor component's [JSInvokable] RenderInBlazor method
    theInstance.invokeMethodAsync('RenderInBlazor');
    // request another animation frame
    window.requestAnimationFrame(renderJS);
}

/*This is called whenever the browser (and therefore the canvas) is resized*/
function WindowResized() {
    // canvasHolder is the ID of the div that wraps the renderfragment
    // content(Canvas) in the blazor app
    // find the canvas within the renderfragment

    // Call the blazor component's [JSInvokable] ResizeInBlazor method
    theInstance.invokeMethodAsync('ResizeInBlazor', window.innerWidth, window.innerHeight);
}

//Handle the canvas.wheel event
// https://developer.mozilla.org/en-US/docs/Web/API/Element/wheel_event
function wheelChange(e) {
    e.preventDefault();
    var args = canvasWheelChangeArgs(e);
    theInstance.invokeMethodAsync('OnWheelChange', args);
}

//Handle the canvas.mouseout event
function mouseOut(e) {
    e.preventDefault();
    var args = canvasMouseMoveArgs(e);
    theInstance.invokeMethodAsync('OnMouseOut', args);
}

//Handle the canvas.mouseout event
function mouseIn(e) {
    e.preventDefault();
    var args = canvasMouseMoveArgs(e);
    theInstance.invokeMethodAsync('OnMouseIn', args);
}

function mouseEnter(e) {
    e.preventDefault();
    var args = canvasMouseMoveArgs(e);
    theInstance.invokeMethodAsync('OnMouseEnter', args);
}

//Handle the window.mousedown event
function mouseDown(e) {
    e.preventDefault();
    var args = canvasMouseMoveArgs(e);
    theInstance.invokeMethodAsync('OnMouseDown', args);
}

//Handle the window.mouseup event
function mouseUp(e) {
    e.preventDefault();
    var args = canvasMouseMoveArgs(e);
    theInstance.invokeMethodAsync('OnMouseUp', args);
}

//Handle the window.mousemove event
function mouseMove(e) {
    e.preventDefault();
    var args = canvasMouseMoveArgs(e);
    theInstance.invokeMethodAsync('OnMouseMove', args);
}

//Handle the canvas.keydown event
function keyDown(e) {
    e.preventDefault();
    var args = canvasKeyboardEventArgs(e);
    theInstance.invokeMethodAsync('OnKeyDown', args);
}

//Handle the canvas.keyup event
function keyUp(e) {
    e.preventDefault();
    var args = canvasKeyboardEventArgs(e);
    theInstance.invokeMethodAsync('OnKeyUp', args);
}

//Handle the canvas.keypress event
function keyPress(e) {
    e.preventDefault();
    var args = canvasKeyboardEventArgs(e);
    theInstance.invokeMethodAsync('OnKeyPress', args);
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

function showFileInputNodeNotFound() {
    window.alert(`No file input node with id='${fileInputId}' was found in CanvasComponent.`);
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
    } else {
        showFileInputNodeNotFound();
    }
}

window.CanvasFileInput = {
    ShowFileInput: showFileInput,
    HideFileInput: hideFileInput,
};

window.initRenderJS = initRenderJS;
