using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;

using BlazorComponentBus;
 
using FoundryBlazor.PubSub;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;


namespace FoundryBlazor.Shared;

public class Canvas2DClassicComponentBase : ComponentBase, IAsyncDisposable, IDisposable
{
    [Inject] public IWorkspace? Workspace { get; set; }
    [Inject] private ComponentBus? PubSub { get; set; }
    [Inject] protected IJSRuntime? _jsRuntime { get; set; }

    [Parameter] public string StyleCanvas { get; set; } = "position: absolute; top: 80px; left: 0px; z-index: 10";
    [Parameter] public int CanvasWidth { get; set; } = 2500;
    [Parameter] public int CanvasHeight { get; set; } = 4000;

    [Parameter] public bool AutoRender { get; set; } = true;

    private int tick = 0;
    private Canvas2DContext? Ctx = null;

    public BECanvasComponent? CanvasReference;
    public JSIntegrationHelper? JSIntegrationRef;

     public async Task DoStart()
    {
        await JSIntegrationRef!.StartAnimation();
    }

    public async Task DoStop()
    {
         await JSIntegrationRef!.StopAnimation();
    }   

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Ctx = await CanvasReference.CreateCanvas2DAsync();

            await JSIntegrationRef!.Initialize();
            // maybe pass in a  reference to a canvas?
            await JSIntegrationRef!.CaptureMouseEventsForCanvas();

            if ( AutoRender )
                await DoStart();

            var drawing = Workspace!.GetDrawing();
            drawing?.SetCanvasSizeInPixels(CanvasWidth, CanvasHeight);

            PubSub!.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);
            PubSub!.SubscribeTo<TriggerRedrawEvent>(OnTriggerRedrawEvent);
        }
        await base.OnAfterRenderAsync(firstRender);
    }


    public void Dispose()
    {

        "Canvas2DClassicComponentBase Dispose".WriteInfo();

        Ctx?.Dispose();

        Ctx = null;
        PubSub!.UnSubscribeFrom<RefreshUIEvent>(OnRefreshUIEvent);
        PubSub!.UnSubscribeFrom<TriggerRedrawEvent>(OnTriggerRedrawEvent);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await JSIntegrationRef!.Finalize();
        "Canvas2DClassicComponentBase DisposeAsync".WriteInfo();
        await DoStop();
    }

    private void OnRefreshUIEvent(RefreshUIEvent e)
    {
        InvokeAsync(StateHasChanged);
        $"Canvas2DClassicComponentBase OnRefreshUIEvent StateHasChanged {e.note}".WriteInfo();
    }

    private void OnTriggerRedrawEvent(TriggerRedrawEvent e)
    {
        Task.Run(async () =>
        {
            await RenderFrame(0);
            $"Canvas2DClassicComponentBase TriggerRedrawEvent StateHasChanged {e.note}".WriteInfo();
        });
    }

    public async Task RenderFrame(double fps)
    {
        if (Ctx == null)
        {
            $"Canvas2D Classic has no context".WriteError();
            return;
        }
        tick++;

        //$"Canvas2D Classic RenderFrame {tick} {fps}".WriteInfo();

        Workspace?.PreRender(tick);

        var drawing = Workspace?.GetDrawing();
        if (drawing == null) return;

        //if you are already rendering then skip it this cycle
        if (drawing.SetCurrentlyRendering(true, tick)) return;
        await Ctx.BeginBatchAsync();
        await Ctx.SaveAsync();

        await drawing.RenderDrawing(Ctx, tick, fps);
        Workspace?.RenderWatermark(Ctx, tick);

        await Ctx.RestoreAsync();
        await Ctx.EndBatchAsync();
        drawing.SetCurrentlyRendering(false, tick);

        Workspace?.PostRender(tick);
    }


}
