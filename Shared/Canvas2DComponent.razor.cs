using Microsoft.AspNetCore.Components;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions;
using BlazorComponentBus;
using FoundryBlazor.Solutions;
using FoundryBlazor.Shape;
using Microsoft.JSInterop;
using FoundryRulesAndUnits.Extensions;
using FoundryBlazor.PubSub;
using System.Text;

namespace FoundryBlazor.Shared;

public class Canvas2DComponentBase : ComponentBase, IAsyncDisposable, IDisposable
{

    [Inject] public IWorkspace? Workspace { get; set; }
    [Inject] private ComponentBus? PubSub { get; set; }
    [Inject] protected IJSRuntime? _jsRuntime { get; set; }

    [Parameter] public string CanvasStyle { get; set; } = "width:max-content; border:1px solid black;cursor:default";

    [Parameter] public int CanvasWidth { get; set; } = 1800;
    [Parameter] public int CanvasHeight { get; set; } = 1200;
    [Parameter] public bool AutoRender { get; set; } = true;
    
    public int tick { get; private set; }
    private DateTime _lastRender;

    public BECanvasComponent? CanvasReference;
    private Canvas2DContext? Ctx;

    public string GetCanvasStyle()
    {
        var style = new StringBuilder(CanvasStyle)
            .Append("; ")
            .Append("width:")
            .Append(CanvasWidth)
            .Append("px; ")
            .Append("height:")
            .Append(CanvasHeight)
            .Append("px; ")
            .ToString();
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
            // SetDoTugOfWar();

            PubSub!.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);
            PubSub!.SubscribeTo<TriggerRedrawEvent>(OnTriggerRedrawEvent);
 
            Ctx = await CanvasReference!.CreateCanvas2DAsync();
            await RenderFrame(0);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            "Canvas2DComponentBase DisposeAsync".WriteInfo();
            //await DoStop();
            //await _jsRuntime!.InvokeVoidAsync("AppBrowser.Finalize");
            await ValueTask.CompletedTask;
        }
        catch (Exception ex)
        {
            $"Canvas2DComponentBase DisposeAsync Exception {ex.Message}".WriteError();
        }
    }

    public void Dispose()
    {
        "Canvas2DComponentBase Dispose".WriteInfo();
        PubSub!.UnSubscribeFrom<RefreshUIEvent>(OnRefreshUIEvent);
        PubSub!.UnSubscribeFrom<TriggerRedrawEvent>(OnTriggerRedrawEvent);
        GC.SuppressFinalize(this);
    }

    public async Task DoStart()
    {
        try {
            await _jsRuntime!.InvokeVoidAsync("AppBrowser.StartAnimation");
        } catch (Exception ex) {
            $"Canvas2DComponentBase DoStart Error {ex.Message}".WriteError();
        }
    }

    public async Task DoStop()
    {
        try {
            await _jsRuntime!.InvokeVoidAsync("AppBrowser.StopAnimation");
        } catch (Exception ex) {
            $"Canvas2DComponentBase DoStop Error {ex.Message}".WriteError();
        }
    }

    [JSInvokable]
    public async ValueTask RenderFrameEventCalled()
    {
        double fps = 1.0 / (DateTime.Now - _lastRender).TotalSeconds;
        _lastRender = DateTime.Now; // update for the next time 

        try
        {
            //recomputing the ctx here look like it is needed and fixes moving between pages
            Ctx = await CanvasReference!.CreateCanvas2DAsync();
            await RenderFrame(fps);
        }
        catch (Exception ex)
        {
            $"Canvas2DComponentBase RenderFrameEventCalled Error {ex.Message}".WriteError();
        }
    }

    private void OnRefreshUIEvent(RefreshUIEvent e)
    {
        InvokeAsync(StateHasChanged);
        //$"Canvas2DComponentBase OnRefreshUIEvent StateHasChanged {e.note}".WriteInfo();
    }
    private void OnTriggerRedrawEvent(TriggerRedrawEvent e)
    {
        Task.Run(async () =>
        {
            await RenderFrame(0);
            $"Canvas2DComponentBase TriggerRedrawEvent StateHasChanged {e.note}".WriteInfo();
        });
    }

    public async Task RenderFrame(double fps)
    {
        if (Ctx == null)
        {
            $"Canvas2D has no context".WriteError();
            return;
        }
        tick++;

        //$"Canvas2D RenderFrame {tick} {fps}".WriteInfo();

        Workspace?.PreRender(tick);

        var drawing = Workspace?.GetDrawing();
        if (drawing == null) return;
        if (drawing.IsFrameRefreshPaused()) return;

        //if you are already rendering then skip it this cycle
        if (drawing.SetCurrentlyRendering(true, tick)) return;
        await Ctx.BeginBatchAsync();

        try
        {
            await Ctx.SaveAsync();
            await drawing.RenderDrawing(Ctx, tick, fps);
            Workspace?.RenderWatermark(Ctx, tick);
            await Ctx.RestoreAsync();
        }
        catch(Exception ex) 
        {
            $"Error {ex.Message}".WriteError();
        }

        await Ctx.EndBatchAsync();
        drawing.SetCurrentlyRendering(false, tick);

        Workspace?.PostRender(tick);
    }


    public void CreateTickPlayground()
    {
        var drawing = Workspace!.GetDrawing();
        if (drawing == null) return;
        var s1 = new FoShape2D(200, 200, "Green");
        drawing.AddShape(s1);
        s1.MoveTo(200, 200);
        var s2 = new FoShape2D(200, 25, "Blue")
        {
            LocPinX = (obj) => obj.Width / 4
        };
        drawing.AddShape(s2);
        s2.ContextLink = (obj, tick) =>
        {
            obj.PinX = s1.PinX;
            obj.PinY = s1.PinY;
            obj.Angle += 1;
        };
    }

    public void SetDoTugOfWar()
    {
        var drawing = Workspace!.GetDrawing();
        if (drawing == null) return;
        var s1 = new FoShape2D(50, 50, "Blue");
        s1.MoveTo(300, 200);
        var s2 = new FoShape2D(50, 50, "Orange");
        s2.MoveTo(500, 200);
        var service = Workspace.GetSelectionService();
        service.AddItem(drawing.AddShape(s1));
        service.AddItem(drawing.AddShape(s2));
        var wire2 = new FoShape1D("Arrow", "Cyan")
        {
            Height = 50,
            ShapeDraw = async (ctx, obj) => await DrawSteveArrowAsync(ctx, obj.Width, obj.Height, obj.Color)
        };
        wire2.GlueStartTo(s1, "RIGHT");
        wire2.GlueFinishTo(s2, "LEFT");
        drawing.AddShape(wire2);
        FoGlyph2D.Animations.Tween<FoShape2D>(s1, new { PinX = s1.PinX - 150, }, 2, 2.2F);
        FoGlyph2D.Animations.Tween<FoShape2D>(s2, new { PinX = s2.PinX + 150, PinY = s2.PinY + 50, }, 2, 2.4f).OnComplete(() =>
        {
            service.ClearAll();
        });
    }

    private static async Task DrawSteveArrowAsync(Canvas2DContext ctx, int width, int height, string color)
    {
        var headWidth = 40;
        var bodyHeight = height / 4;
        var bodyWidth = width - headWidth;
        await ctx.SetFillStyleAsync(color);
        var y = (height - bodyHeight) / 2.0;
        await ctx.FillRectAsync(0, y, bodyWidth, bodyHeight);
        await ctx.BeginPathAsync();
        await ctx.MoveToAsync(bodyWidth, 0);
        await ctx.LineToAsync(width, height / 2);
        await ctx.LineToAsync(bodyWidth, height);
        await ctx.LineToAsync(bodyWidth, 0);
        await ctx.ClosePathAsync();
        await ctx.FillAsync();
        await ctx.SetFillStyleAsync("#fff");
        await ctx.FillTextAsync("→", width / 2, height / 2, 20);
    }




}
