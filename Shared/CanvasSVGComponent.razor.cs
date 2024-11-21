using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
 
using FoundryBlazor.Solutions;
using FoundryBlazor.Shape;
using Microsoft.JSInterop;
using FoundryBlazor.PubSub;
using FoundryRulesAndUnits.Extensions;
using System.Text;

namespace FoundryBlazor.Shared;

public class CanvasSVGComponentBase : ComponentBase, IAsyncDisposable, IDisposable
{
    [Inject] public IHitTestService? HitTestService { get; set; }
    [Inject] public IWorkspace? Workspace { get; set; }
    [Inject] private ComponentBus? PubSub { get; set; }
    [Inject] protected IJSRuntime? _jsRuntime { get; set; }
    [Parameter] public string CanvasStyle { get; set; } = "width:max-content; border:1px solid black;cursor:default";

    [Parameter] public int CanvasWidth { get; set; } = 1800;
    [Parameter] public int CanvasHeight { get; set; } = 1200;
    [Parameter] public bool AutoRender { get; set; } = true;


    protected string CurrentKey { get; set; } = "";
    public int tick { get; private set; }

    private DateTime _lastRender;

    public string GetCanvasStyle()
    {
        var style = new StringBuilder(CanvasStyle).Append("; ").Append("width:").Append(CanvasWidth).Append("px; ").Append("height:").Append(CanvasHeight).Append("px; ").ToString();
        return style;
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _jsRuntime!.InvokeVoidAsync("AppBrowser.Initialize", DotNetObjectReference.Create(this));
 
            var drawing = Workspace!.GetDrawing();
            drawing?.SetCanvasSizeInPixels(CanvasWidth, CanvasHeight);
            if ( AutoRender)
                await DoStart();
                
            // CreateTickPlayground();

            PubSub!.SubscribeTo<TriggerRedrawEvent>(OnTriggerRedrawEvent);
            PubSub!.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);

        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            //await DoStop();
            "CanvasSVGComponentBase DisposeAsync".WriteInfo();
            //await _jsRuntime!.InvokeVoidAsync("AppBrowser.Finalize");
            await ValueTask.CompletedTask;
        }
        catch (Exception ex)
        {
            $"CanvasSVGComponentBase DisposeAsync Exception {ex.Message}".WriteError();
        }


    }

    public void Dispose()
    {
        PubSub!.UnSubscribeFrom<RefreshUIEvent>(OnRefreshUIEvent);
        PubSub!.UnSubscribeFrom<TriggerRedrawEvent>(OnTriggerRedrawEvent);
        GC.SuppressFinalize(this);
    }
    private void OnRefreshUIEvent(RefreshUIEvent e)
    {
        InvokeAsync(StateHasChanged);
        $"CanvasSVGComponentBase OnRefreshUIEvent StateHasChanged {e.note}".WriteInfo();
    }

    [JSInvokable]
    public async ValueTask RenderFrameEventCalled()
    {
        double fps = 1.0 / (DateTime.Now - _lastRender).TotalSeconds;
        _lastRender = DateTime.Now; // update for the next time 

        try
        {
            await RenderFrame(fps);
        }
        catch (Exception ex)
        {
            $"CanvasSVGComponentBase RenderFrameEventCalled Error {ex.Message}".WriteError();
        }
    }

    private void OnTriggerRedrawEvent(TriggerRedrawEvent e)
    {
        Task.Run(async () =>
        {
            await RenderFrame(0);
            $"CanvasSVGComponentBase TriggerRedrawEvent StateHasChanged {e.note}".WriteInfo();
        });
    }

    public async Task RenderFrame(double fps)
    {
        tick++;
        Workspace?.PreRender(tick);

        var drawing = Workspace?.GetDrawing();
        if (drawing == null) return;
        if (drawing.IsFrameRefreshPaused()) return;

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
