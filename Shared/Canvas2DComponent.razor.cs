﻿using Microsoft.AspNetCore.Components;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions;
using BlazorComponentBus;
using FoundryBlazor.Solutions;
using FoundryBlazor.Shape;
using Microsoft.JSInterop;
using FoundryRulesAndUnits.Extensions;
using FoundryBlazor.PubSub;

namespace FoundryBlazor.Shared;

public class Canvas2DComponentBase : ComponentBase
{

    [Inject] public IWorkspace? Workspace { get; set; }
    [Inject] private ComponentBus? PubSub { get; set; }
    [Inject] protected IJSRuntime? _jsRuntime { get; set; }
    [Parameter] public int CanvasWidth { get; set; } = 1800;
    [Parameter] public int CanvasHeight { get; set; } = 1200;
    [Parameter] public bool AutoRender { get; set; } = true;
    
    public int tick { get; private set; }
    private DateTime _lastRender;

    public BECanvasComponent? CanvasReference;
    private Canvas2DContext? Ctx;


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _jsRuntime!.InvokeVoidAsync("AppBrowser.SetDotNetObjectReference", DotNetObjectReference.Create(this));
 
            Ctx = await CanvasReference!.CreateCanvas2DAsync();

            var drawing = Workspace!.GetDrawing();
            drawing?.SetCanvasSizeInPixels(CanvasWidth, CanvasHeight);
            if ( AutoRender)
                await DoStart();
            // CreateTickPlayground();
            // SetDoTugOfWar();

            PubSub!.SubscribeTo<TriggerRedrawEvent>(OnTriggerRedrawEvent);
 
        }
        await base.OnAfterRenderAsync(firstRender);
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
            $"RenderFrameEventCalled Error {ex.Message}".WriteError();
        }
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
            //Ctx = await CanvasReference.CreateCanvas2DAsync();
            $"Canvas2D has no context".WriteError();
            return;
        }
        tick++;

        //$"Canvas2D RenderFrame {tick} {fps}".WriteInfo();

        Workspace?.PreRender(tick);

        var drawing = Workspace?.GetDrawing();
        if (drawing == null) return;

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


    private void CreateTickPlayground()
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

    private void SetDoTugOfWar()
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

    public async Task DoStart()
    {
        await _jsRuntime!.InvokeVoidAsync("AppBrowser.StartAnimation");
    }

    public async Task DoStop()
    {
        await _jsRuntime!.InvokeVoidAsync("AppBrowser.StopAnimation");
    }

}
