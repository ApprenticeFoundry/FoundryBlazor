/*
 * Adapted from Scott Harden's EXCELLENT blog post, 
 * "Draw Animated Graphics in the Browser with Blazor WebAssembly"
 * https://swharden.com/blog/2021-01-07-blazor-canvas-animated-graphics/
 */

using System.Drawing;
using BlazorComponentBus;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FoundryBlazor.Canvas
{
    /// <summary>
    /// CanvasHelper component gives you render and resize callbacks for Canvas animation
    /// </summary>
    public partial class CanvasHelper : ComponentBase //, IAsyncDisposable
    {
        [Inject] private ComponentBus? PubSub { get; set; }
        /// <summary>
        /// JS Interop module used to call JavaScript
        /// </summary>
        private Lazy<Task<IJSObjectReference>>? _moduleTask;

        /// <summary>
        /// Used to calculate the frames per second
        /// </summary>
        private DateTime _lastRender;

        /// <summary>
        /// JS Runtime
        /// </summary>
        [Inject]
        protected IJSRuntime? _jsRuntime { get; set; }



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
        /// Event called on mouse out
        /// </summary>
        [Parameter]
        public EventCallback<CanvasMouseArgs> MouseOut { get; set; }

        /// <summary>
        /// Event called on mouse in
        /// </summary>
        [Parameter]
        public EventCallback<CanvasMouseArgs> MouseIn { get; set; }

        /// <summary>
        /// Event called on mouse enter
        /// </summary>
        [Parameter]
        public EventCallback<CanvasMouseArgs> MouseEnter { get; set; }

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

        [Parameter]
        public EventCallback<CanvasDragArgs> DragStart { get; set; }
        [Parameter]
        public EventCallback<CanvasDragArgs> Drag { get; set; }
        [Parameter]
        public EventCallback<CanvasDragArgs> DragEnd { get; set; }
        [Parameter]
        public EventCallback<CanvasDragArgs> DragEnter { get; set; }
        [Parameter]
        public EventCallback<CanvasDragArgs> DragOver { get; set; }
        [Parameter]
        public EventCallback<CanvasDragArgs> DragLeave { get; set; }
        [Parameter]
        public EventCallback<CanvasDragArgs> Drop { get; set; }
        /// <summary>
        /// Call this in your Blazor app's OnAfterRenderAsync method when firstRender is true
        /// </summary>
        /// <returns></returns>
        public async Task Initialize()
        {
            // Initialize
            await _jsRuntime!.InvokeVoidAsync("initRenderJS", DotNetObjectReference.Create(this));
        }


        [JSInvokable]
        public async Task ResizeInBlazor(int width, int height)
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
        public async ValueTask RenderInBlazor()
        {
            // calculate frames per second
            double fps = 1.0 / (DateTime.Now - _lastRender).TotalSeconds;

            _lastRender = DateTime.Now; // update for the next time 

            // raise the RenderFrame event to the blazor app
            await RenderFrame.InvokeAsync(fps);
        }


        [JSInvokable]
        public async Task OnWheelChange(CanvasWheelChangeArgs args)
        {
            args.Topic = "ON_WHEEL_CHANGE";

            PubSub?.Publish<CanvasWheelChangeArgs>(args);
            await WheelChange.InvokeAsync(args);
        }


        [JSInvokable]
        public async Task OnMouseOut(CanvasMouseArgs args)
        {
            args.Topic = "ON_MOUSE_OUT";
            PubSub?.Publish<CanvasMouseArgs>(args);
            await MouseOut.InvokeAsync(args);
        }


        [JSInvokable]
        public async Task OnMouseIn(CanvasMouseArgs args)
        {
            args.Topic = "ON_MOUSE_IN";
            PubSub?.Publish<CanvasMouseArgs>(args);
            await MouseIn.InvokeAsync(args);
        }

        [JSInvokable]
        public async Task OnMouseEnter(CanvasMouseArgs args)
        {
            // We *must* fire this when fileInputContainer is hidden after a file is dropped.
            args.Topic = "ON_MOUSE_ENTER";
            PubSub?.Publish<CanvasMouseArgs>(args);
            await MouseEnter.InvokeAsync(args);
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

        /// <summary>
        /// Handle the JavaScript canvas.keyup event
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [JSInvokable]
        public async Task OnKeyUp(CanvasKeyboardEventArgs args)
        {
            args.Topic = "ON_KEY_UP";
            PubSub?.Publish<CanvasKeyboardEventArgs>(args);
            await KeyUp.InvokeAsync(args);
        }

        /// <summary>
        /// Handle the JavaScript canvas.keypress event
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [JSInvokable]
        public async Task OnKeyPress(CanvasKeyboardEventArgs args)
        {
            args.Topic = "ON_KEY_PRESS";
            PubSub?.Publish<CanvasKeyboardEventArgs>(args);
            await KeyPress.InvokeAsync(args);
        }

        [JSInvokable]
        public async Task OnDragStart(CanvasDragArgs args)
        {
            args.Topic = "ON_DRAG_START";
            PubSub?.Publish<CanvasDragArgs>(args);
            await DragStart.InvokeAsync(args);
        }

        [JSInvokable]
        public async Task OnDrag(CanvasDragArgs args)
        {
            args.Topic = "ON_DRAG";
            PubSub?.Publish<CanvasDragArgs>(args);
            await Drag.InvokeAsync(args);
        }
        [JSInvokable]
        public async Task OnDragEnd(CanvasDragArgs args)
        {
            args.Topic = "ON_DRAG_END";
            PubSub?.Publish<CanvasDragArgs>(args);
            await DragEnd.InvokeAsync(args);
        }
        [JSInvokable]
        public async Task OnDragEnter(CanvasDragArgs args)
        {
            args.Topic = "ON_DRAG_ENTER";
            PubSub?.Publish<CanvasDragArgs>(args);
            await DragEnter.InvokeAsync(args);
        }
        [JSInvokable]
        public async Task OnDragOver(CanvasDragArgs args)
        {
            args.Topic = "ON_DRAG_OVER";
            PubSub?.Publish<CanvasDragArgs>(args);
            await DragOver.InvokeAsync(args);
        }
        [JSInvokable]
        public async Task OnDragLeave(CanvasDragArgs args)
        {
            args.Topic = "ON_DRAG_LEAVE";
            PubSub?.Publish<CanvasDragArgs>(args);
            await DragLeave.InvokeAsync(args);
        }
        [JSInvokable]
        public async Task OnDrop(CanvasDragArgs args)
        {
            args.Topic = "ON_DROP";
            PubSub?.Publish<CanvasDragArgs>(args);
            await Drop.InvokeAsync(args);
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
