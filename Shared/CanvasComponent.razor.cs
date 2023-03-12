using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;

using BlazorComponentBus;
using FoundryBlazor.Canvas;
using FoundryBlazor.Extensions;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Radzen.Blazor.Rendering;

namespace FoundryBlazor.Shared;

public class CanvasComponentBase : ComponentBase, IDisposable
{
    [Inject] public IWorkspace? Workspace { get; set; }
    [Inject] private ComponentBus? PubSub { get; set; }
    [Inject] protected IJSRuntime? JsRuntime { get; set; }
    private int tick = 0;
    public int CanvasWidth = 2500;
    public int CanvasHeight = 4000;
    private Canvas2DContext? Ctx;
    public BECanvasComponent? CanvasReference;
    public CanvasHelper? CanvasHelperReference;
    private IBrowserFile? InputFile;
    private bool IsUploading = false;


    public void Dispose()
    {
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
            await CanvasHelperReference!.Initialize();
            var drawing = Workspace!.GetDrawing();
            drawing?.SetCanvasSize(CanvasWidth, CanvasHeight);
            drawing?.SetPanID(Workspace.GetPanID());

            PubSub!.SubscribeTo<CanvasMouseArgs>(OnCanvasMouseEvent);
        }
    }

    public void OnFileInputChange(InputFileChangeEventArgs e)
    {
        InputFile = e.File;
        $"{InputFile.Name} {InputFile.Size}".WriteLine(ConsoleColor.Green);
        //Task.Run(async () =>
        //{
        //    await JsRuntime!.InvokeVoidAsync("CanvasFileInput.HideFileInput");
        //});

    }

    private void OnCanvasMouseEvent(CanvasMouseArgs MouseArgs)
    {
        if (InputFile != null && IsUploading == false)
        {
            IsUploading = true;
            $"OnCanvasMouseEvent {MouseArgs.OffsetX} {MouseArgs.OffsetY}".WriteLine(ConsoleColor.Green);
            Task.Run(async () =>
            {
                //await JsRuntime!.InvokeVoidAsync("CanvasFileInput.ShowFileInput");
                await Workspace!.DropFileCreateShape(InputFile, MouseArgs);
                InputFile = null;
                IsUploading = false;
            });
        }
    }



    public async Task RenderFrame(double fps)
    {
        if (Ctx == null) return;
        tick++;

        Workspace?.PreRender(tick);

        var drawing = Workspace?.GetDrawing();
        if (drawing == null) return;

        //if you are already rendering then skip it this cycle
        if ( drawing.SetCurrentlyRendering(true) ) return;
        await Ctx.BeginBatchAsync();
        await Ctx.SaveAsync();

        await drawing.RenderDrawing(Ctx, tick, fps);
        Workspace?.RenderWatermark(Ctx, tick);
        
        await Ctx.RestoreAsync();
        await Ctx.EndBatchAsync();
        drawing.SetCurrentlyRendering(false);

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
