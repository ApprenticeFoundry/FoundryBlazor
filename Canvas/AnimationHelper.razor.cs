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
    public partial class AnimationHelper : ComponentBase //, IAsyncDisposable
    {

        /// <summary>
        /// Used to calculate the frames per second
        /// </summary>
        private DateTime _lastRender;

        /// <summary>
        /// JS Runtime
        /// </summary>
        [Inject]
        protected IJSRuntime? _jsRuntime { get; set; }



        /// <summary>
        /// Event called every time a frame can be redrawn
        /// </summary>
        [Parameter]
        public EventCallback<double> RenderFrame { get; set; }


        public async Task Initialize()
        {
            // Initialize
            await _jsRuntime!.InvokeVoidAsync("initAnimationJS", DotNetObjectReference.Create(this));
        }





        [JSInvokable]
        public async ValueTask RenderAnimation()
        {
            // calculate frames per second
            double fps = 1.0 / (DateTime.Now - _lastRender).TotalSeconds;

            _lastRender = DateTime.Now; // update for the next time 

            // raise the RenderFrame event to the blazor app
            await RenderFrame.InvokeAsync(fps);
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
