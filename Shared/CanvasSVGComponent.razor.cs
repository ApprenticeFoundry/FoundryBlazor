using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
using FoundryBlazor.Canvas;
using FoundryBlazor.Solutions;
using FoundryBlazor.Shape;
using Microsoft.JSInterop;

namespace FoundryBlazor.Shared;

public class CanvasSVGComponentBase : ComponentBase
{
    [Inject] public IHitTestService? HitTestService { get; set; }
    [Inject] public IWorkspace? Workspace { get; set; }
    [Inject] private ComponentBus? PubSub { get; set; }
    [Inject] protected IJSRuntime? _jsRuntime { get; set; }
    [Parameter] public int CanvasWidth { get; set; } = 1800;
    [Parameter] public int CanvasHeight { get; set; } = 1200;
    [Parameter] public string StyleCanvas { get; set; } = "background-color:lightsteelblue";

    protected string CurrentKey { get; set; } = "";
    public int tick { get; private set; }

    private DateTime _lastRender;


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _jsRuntime!.InvokeVoidAsync("AppBrowser.SetDotNetObjectReference", DotNetObjectReference.Create(this));
            //    await _jsRuntime!.InvokeVoidAsync("initJSIntegration", DotNetObjectReference.Create(this));

            var drawing = Workspace!.GetDrawing();
            drawing?.SetCanvasSizeInPixels(CanvasWidth, CanvasHeight);
        
            //await _jsRuntime!.InvokeVoidAsync("AppBrowser.StartAnimation");
            // CreateTickPlayground();

        }
        await base.OnAfterRenderAsync(firstRender);
    }

    [JSInvokable]
    public async ValueTask RenderFrameEventCalled()
    {
        double fps = 1.0 / (DateTime.Now - _lastRender).TotalSeconds;
        _lastRender = DateTime.Now; // update for the next time 

        await RenderFrame(fps);
    }

    public async Task RenderFrame(double fps)
    {
        tick++;
        Workspace?.PreRender(tick);

        var drawing = Workspace?.GetDrawing();
        if (drawing == null) return;

        //if you are already rendering then skip it this cycle
        if (drawing.SetCurrentlyRendering(true, tick)) return;

        // await drawing.RenderDrawing(Ctx, tick, fps);
        drawing.RenderDrawingSVG(tick, fps);
        await InvokeAsync(StateHasChanged);
        // Workspace?.RenderWatermark(Ctx, tick);

        drawing.SetCurrentlyRendering(false, tick);

        Workspace?.PostRender(tick);
    }

    public FoPage2D GetCurrentPage()
    {
        return Workspace!.CurrentPage();
    }



    public async Task DoStart()
    {
        // await _jsRuntime!.InvokeVoidAsync("StartAnimation");
        await _jsRuntime!.InvokeVoidAsync("AppBrowser.StartAnimation");
    }

    public async Task DoStop()
    {
        // await _jsRuntime!.InvokeVoidAsync("StopAnimation");
        await _jsRuntime!.InvokeVoidAsync("AppBrowser.StopAnimation");
    }

}
