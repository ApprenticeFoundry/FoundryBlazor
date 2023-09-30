﻿/*
 * Adapted from Scott Harden's EXCELLENT blog post, 
 * "Draw Animated Graphics in the Browser with Blazor WebAssembly"
 * https://swharden.com/blog/2021-01-07-blazor-canvas-animated-graphics/
 */

using System.Drawing;
using BlazorComponentBus;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FoundryBlazor.Canvas
{
    /// <summary>
    /// CanvasHelper component gives you render and resize callbacks for Canvas animation
    /// </summary>
    public partial class JSIntegrationHelper : ComponentBase //, IAsyncDisposable
    {
        private DateTime _lastRender;

        [Inject] private ComponentBus? PubSub { get; set; }

        [Inject] protected IJSRuntime? _jsRuntime { get; set; }



        /// <summary>RenderFrame
        /// Event called when the browser (and therefore the canvas) is resized
        /// </summary>
        [Parameter]
        public EventCallback<Size> UserWindowResized { get; set; }

        /// <summary>
        /// Event called every time a frame can be redrawn
        /// </summary>
        [Parameter]
        public EventCallback<double> RenderFrame { get; set; }

        /// <summary>
        /// Event called on wheel rotation
        /// </summary>
        [Parameter]
        public EventCallback<CanvasWheelChangeArgs> WheelChange { get; set; }

        /// <summary>
        /// Event called on mouse down
        /// </summary>
        [Parameter]
        public EventCallback<CanvasMouseArgs> MouseDown { get; set; }

        /// <summary>
        /// Event called on mouse up
        /// </summary>
        [Parameter]
        public EventCallback<CanvasMouseArgs> MouseUp { get; set; }

        /// <summary>
        /// Event called on mouse move
        /// </summary>
        [Parameter]
        public EventCallback<CanvasMouseArgs> MouseMove { get; set; }

        /// <summary>
        /// Event called on key down
        /// </summary>
        [Parameter]
        public EventCallback<CanvasKeyboardEventArgs> KeyDown { get; set; }

        /// <summary>
        /// Event called on key up
        /// </summary>
        [Parameter]
        public EventCallback<CanvasKeyboardEventArgs> KeyUp { get; set; }

        /// <summary>
        /// Event called on key press
        /// </summary>
        [Parameter]
        public EventCallback<CanvasKeyboardEventArgs> KeyPress { get; set; }


        public async Task Initialize()
        {
            await _jsRuntime!.InvokeVoidAsync("initJSIntegration", DotNetObjectReference.Create(this));
        }

        public async Task CaptureMouseEventsForCanvas()
        {
            await _jsRuntime!.InvokeVoidAsync("CaptureMouseEventsForCanvas");
        }
        public async Task RemoveMouseEventsForCanvas()
        {
            await _jsRuntime!.InvokeVoidAsync("RemoveMouseEventsForCanvas");
        }

        public async Task CaptureMouseEventsForSVG()
        {
            await _jsRuntime!.InvokeVoidAsync("CaptureMouseEventsForSVG");
        }
        public async Task RemoveMouseEventsForSVG()
        {
            await _jsRuntime!.InvokeVoidAsync("RemoveMouseEventsForSVG");
        }

        public async Task StartAnimation()
        {
            await _jsRuntime!.InvokeVoidAsync("StartAnimation");
        }
        public async Task StopAnimation()
        {
            await _jsRuntime!.InvokeVoidAsync("StopAnimation");
        }

        [JSInvokable]
        public async Task ResizeWindowEventCalled(int width, int height)
        {
            var size = new Size(width, height);
            // Raise the CanvasResized event to the Blazor app
            var args = new CanvasResizeArgs()
            {
                Topic = "ON_USERWINDOW_RESIZE",
                size = size
            };
            PubSub?.Publish<CanvasResizeArgs>(args);
            await UserWindowResized.InvokeAsync(size);
        }


        [JSInvokable]
        public async ValueTask RenderFrameEventCalled()
        {
            // calculate frames per second
            double fps = 1.0 / (DateTime.Now - _lastRender).TotalSeconds;

            _lastRender = DateTime.Now; // update for the next time 

            // raise the RenderFrame event to the blazor app
            try {
                await RenderFrame.InvokeAsync(fps);
            } 
            catch(Exception ex) 
            {
                $"JSIntegrationHelper RenderFrameEventCalled Error {ex.Message}".WriteError();
            }
        }


        [JSInvokable]
        public async Task OnWheelChange(CanvasWheelChangeArgs args)
        {
            args.Topic = "ON_WHEEL_CHANGE";

            PubSub?.Publish<CanvasWheelChangeArgs>(args);
            await WheelChange.InvokeAsync(args);
        }


        [JSInvokable]
        public async Task OnMouseDown(CanvasMouseArgs args)
        {
            args.Topic = "ON_MOUSE_DOWN";

            PubSub?.Publish<CanvasMouseArgs>(args);
            await MouseDown.InvokeAsync(args);
        }


        [JSInvokable]
        public async Task OnMouseUp(CanvasMouseArgs args)
        {
            args.Topic = "ON_MOUSE_UP";
            PubSub?.Publish<CanvasMouseArgs>(args);
            await MouseUp.InvokeAsync(args);
        }


        [JSInvokable]
        public async Task OnMouseMove(CanvasMouseArgs args)
        {
            args.Topic = "ON_MOUSE_MOVE";
            PubSub?.Publish<CanvasMouseArgs>(args);
            await MouseMove.InvokeAsync(args);
        }


        [JSInvokable]
        public async Task OnKeyDown(CanvasKeyboardEventArgs args)
        {
            args.Topic = "ON_KEY_DOWN";
            PubSub?.Publish<CanvasKeyboardEventArgs>(args);
            await KeyDown.InvokeAsync(args);
        }


        [JSInvokable]
        public async Task OnKeyUp(CanvasKeyboardEventArgs args)
        {
            args.Topic = "ON_KEY_UP";
            PubSub?.Publish<CanvasKeyboardEventArgs>(args);
            await KeyUp.InvokeAsync(args);
        }


        [JSInvokable]
        public async Task OnKeyPress(CanvasKeyboardEventArgs args)
        {
            args.Topic = "ON_KEY_PRESS";
            PubSub?.Publish<CanvasKeyboardEventArgs>(args);
            await KeyPress.InvokeAsync(args);
        }



        /// <summary>
        /// Dispose of our module resource
        /// </summary>
        /// <returns></returns>
        //public async ValueTask DisposeAsync()
        //{
        //    if (_moduleTask != null && _moduleTask.IsValueCreated)
        //    {
        //        var module = await _moduleTask.Value;
        //        await module.DisposeAsync();
        //    }
        //}
    }
}
