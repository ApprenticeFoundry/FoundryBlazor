using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;

using BlazorComponentBus;
using FoundryBlazor.Canvas;
using FoundryBlazor.PubSub;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;


namespace FoundryBlazor.Shared;

public class Canvas2DComponentBase : ComponentBase, IDisposable
{
    [Inject] public IWorkspace? Workspace { get; set; }
    [Inject] private ComponentBus? PubSub { get; set; }
    [Inject] protected IJSRuntime? JsRuntime { get; set; }

    [Parameter] public string StyleCanvas { get; set; } = "position: absolute; top: 80px; left: 0px; z-index: 10";
    [Parameter] public string StyleDrop { get; set; } = "position: absolute; top: 100px; left: 20px; z-index: 0; border: 6px dashed red";
    [Parameter] public int CanvasWidth { get; set; } = 2500;
    [Parameter] public int CanvasHeight { get; set; } = 4000;
    private int tick = 0;
    private Canvas2DContext? Ctx;
    public BECanvasComponent? CanvasReference;
    public JSIntegrationHelper? JSIntegrationRef;
    private IBrowserFile? InputFile;
    private bool IsUploading = false;


    public void Dispose()
    {
        Ctx = null;
        //Dispose(true);
        PubSub!.UnSubscribeFrom<CanvasMouseArgs>(OnCanvasMouseEvent);

        // This object will be cleaned up by the Dispose method.
        // Therefore, you should call GC.SupressFinalize to
        // take this object off the finalization queue 
        // and prevent finalization code for this object
        // from executing a second time.
        GC.SuppressFinalize(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Ctx = await CanvasReference.CreateCanvas2DAsync();
            //PubSub!.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);

            await JSIntegrationRef!.Initialize();
            // maybe pass in a  reference to a canvas?
            await JSIntegrationRef!.CaptureMouseEventsForCanvas();
            await JSIntegrationRef!.StartAnimation();

            var drawing = Workspace!.GetDrawing();
            drawing?.SetCanvasPixelSize(CanvasWidth, CanvasHeight);


            PubSub!.SubscribeTo<CanvasMouseArgs>(OnCanvasMouseEvent);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public void OnFileInputChange(InputFileChangeEventArgs e)
    {
        InputFile = e.File;
        $"OnFileInputChange {InputFile.Name} {InputFile.Size} {DateTime.Now.ToLongTimeString()}".WriteInfo();

        CaptureFileAndSend(new CanvasMouseArgs()
        {
            OffsetX = 300,
            OffsetY = 300
        });
        //Task.Run(async () =>
        //{
        //    await JsRuntime!.InvokeVoidAsync("CanvasFileInput.HideFileInput");
        //});

    }

    private void CaptureFileAndSend(CanvasMouseArgs MouseArgs)
    {
        if (InputFile != null && IsUploading == false)
        {
            IsUploading = true;
            $"DropFileCreateShape OnCanvasMouseEvent {MouseArgs.OffsetX} {MouseArgs.OffsetY} {DateTime.Now.ToLongTimeString()}".WriteInfo();
            Task.Run(async () =>
            {
                //await JsRuntime!.InvokeVoidAsync("CanvasFileInput.ShowFileInput");
                await Workspace!.DropFileCreateShape(InputFile, MouseArgs);
                InputFile = null;
                IsUploading = false;
            });
        }
    }

    private void OnCanvasMouseEvent(CanvasMouseArgs MouseArgs)
    {
        CaptureFileAndSend(MouseArgs);
    }

    private void OnRefreshUIEvent(RefreshUIEvent e)
    {
        InvokeAsync(StateHasChanged);
        $"Canvas2DComponentBase OnRefreshUIEvent StateHasChanged {e.note}".WriteInfo();
    }

    public async Task RenderFrame(double fps)
    {
        if (Ctx == null) return;
        tick++;

        //$"Canvas2D RenderFrame {tick} {fps}".WriteInfo();

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

    public string FileInputStyle()
    {
        var w = $"{CanvasWidth - 40}px";
        var h = $"{CanvasHeight - 40}px";
        var style = $"opacity:0; width:{w}; height:{h}";
        return style;
    }

}
