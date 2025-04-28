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



public class Canvas2DComponentBase : ComponentBase, IAsyncDisposable
{

    [Inject] public IWorkspace? Workspace { get; set; }
    [Inject] private ComponentBus? PubSub { get; set; }

    [Inject] protected IFoundryService? FoundryService { get; set; }
    [Inject] protected IJSRuntime? _jsRuntime { get; set; }

    [Parameter] public int CanvasWidth { get; set; } = 1800;
    [Parameter] public int CanvasHeight { get; set; } = 1200;

    [Parameter] public bool WithAnimations { get; set; } = true;
    [Parameter, EditorRequired] public string? SceneName { get; set; }

    public int tick { get; private set; }

    protected BECanvasComponent? BECanvasReference;

    private Canvas2DContext? Ctx;



    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            $"Canvas2DComponentBase {SceneName} OnAfterRenderAsync".WriteInfo();

            await _jsRuntime!.InvokeVoidAsync("AppBrowser.Initialize");

            var drawing = Workspace!.GetDrawing();
            drawing?.ClearAll();  //we do not want to share the old drawing here

            drawing?.SetCanvasSizeInPixels(CanvasWidth, CanvasHeight);

            //lets hope the reference to BECanvas was found
            Ctx = await BECanvasReference!.CreateCanvas2DAsync();


            //CreateTickPlayground();
            //SetDoTugOfWar();

            PubSub?.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);
            PubSub?.SubscribeTo<TriggerRedrawEvent>(OnTriggerRedrawEvent);
            FoundryService?.AnimationBus().SubscribeTo<AnimationEvent>(OnAnimationEvent);

            await RenderFrame(0);
            if (WithAnimations)
                await DoStart();

        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (Ctx == null) return;
            Ctx = null;

            $"Canvas2DComponentBase {SceneName} DisposeAsync".WriteInfo();
            if (WithAnimations)
                await DoStop();

            PubSub?.UnSubscribeFrom<RefreshUIEvent>(OnRefreshUIEvent);
            PubSub?.UnSubscribeFrom<TriggerRedrawEvent>(OnTriggerRedrawEvent);
            FoundryService?.AnimationBus().UnSubscribeFrom<AnimationEvent>(OnAnimationEvent);

            await _jsRuntime!.InvokeVoidAsync("AppBrowser.Finalize");

        }
        catch (Exception ex)
        {
            $"Canvas2DComponentBase DisposeAsync Exception {ex.Message}".WriteError();
        }
    }



    public async Task DoStart()
    {
        try
        {

            $"Canvas2DComponentBase {SceneName} CALLING DO START  AppBrowser.StartAnimation".WriteSuccess();
            await _jsRuntime!.InvokeVoidAsync("AppBrowser.StartAnimation");

        }
        catch (Exception ex)
        {
            $"Canvas2DComponentBase DoStart Error {ex.Message}".WriteError();
        }
    }

    public async Task DoStop()
    {
        try
        {

            $"Canvas2DComponentBase {SceneName} CALLING DO STOP  AppBrowser.StopAnimation".WriteSuccess();
            await _jsRuntime!.InvokeVoidAsync("AppBrowser.StopAnimation");

        }
        catch (Exception ex)
        {
            $"Canvas2DComponentBase DoStop Error {ex.Message}".WriteError();
        }
    }



    protected async void OnAnimationEvent(AnimationEvent message)
    {
        await RenderFrame(message.fps);
    }



    private void OnRefreshUIEvent(RefreshUIEvent e)
    {
        InvokeAsync(StateHasChanged);
        //$"Canvas2DComponentBase OnRefreshUIEvent StateHasChanged {e.note}".WriteInfo();
    }

    private void OnTriggerRedrawEvent(TriggerRedrawEvent e)
    {
        Render();
    }

    public void Render()
    {
        Task.Run(async () => await RenderFrame(0));
    }

    public async Task RenderFrame(double fps)
    {
        if (Ctx == null)
        {
            $"Canvas2DComponentBase has no context".WriteError();
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
        catch (Exception ex)
        {
            $"RenderFrame Error {ex.Message}".WriteError();
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
