using BlazorComponentBus;
using FoundryBlazor.Canvas;
using FoundryBlazor.PubSub;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;

namespace FoundryBlazor.Shared;

public class CanvasSVGComponentBase : ComponentBase, IDisposable
{
    [Inject] public IWorkspace? Workspace { get; set; }
    [Inject] private ComponentBus? PubSub { get; set; }
    [Inject] protected IJSRuntime? JsRuntime { get; set; }

    [Parameter] public string StyleCanvas { get; set; } = "position: absolute; top: 80px; left: 0px; z-index: 10";
    [Parameter] public string StyleDrop { get; set; } = "position: absolute; top: 100px; left: 20px; z-index: 0; border: 6px dashed red";
    [Parameter] public int CanvasWidth { get; set; } = 2500;
    [Parameter] public int CanvasHeight { get; set; } = 4000;


    private int tick = 0;


    public JSIntegrationHelper? JSIntegrationRef;
    private IBrowserFile? InputFile;
    private bool IsUploading = false;


    protected override void OnInitialized()
    {

    }

    protected override void OnParametersSet()
    {
        $"CanvasSVGComponent OnParametersSet".WriteInfo(2);
        base.OnParametersSet();
    }

    private void OnSelectionChanged(SelectionChanged e)
    {
        $"CanvasSVGComponent.OnSelectionChanged e.State={e.State}".WriteInfo(2);
        if (e.State == SelectionState.Dropped)
        {

        }
    }

    public FoPage2D GetCurrentPage()
    {
        return Workspace!.CurrentPage();
    } 

    public void Dispose()
    {
        PubSub!.UnSubscribeFrom<RefreshUIEvent>(OnRefreshUIEvent);
        PubSub!.UnSubscribeFrom<CanvasMouseArgs>(OnCanvasMouseEvent);
        PubSub!.UnSubscribeFrom<SelectionChanged>(OnSelectionChanged);

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
            PubSub!.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);
            PubSub!.SubscribeTo<SelectionChanged>(OnSelectionChanged);

            await JSIntegrationRef!.Initialize();
            // maybe pass in a  reference to a SVG?
            await JSIntegrationRef!.CaptureMouseEventsForSVG();
            
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
    }

    public void RenderFrame(double fps)
    {
        tick++;

        $"CanvasSVG RenderFrame {tick} {fps}".WriteInfo();

        Workspace?.PreRender(tick);

        var drawing = Workspace?.GetDrawing();
        if (drawing == null) return;

        //if you are already rendering then skip it this cycle
        if (drawing.SetCurrentlyRendering(true, tick)) return;

        // Nodes = new();

        // var mtx = PanZoom?.GetMatrix() ?? new Matrix2D();
        // PagePanZoom = $"matrix({mtx.a}, {mtx.b}, {mtx.c}, {mtx.d}, {mtx.tx}, {mtx.ty})";

        // $"CanvasSVG RenderFrame {tick} {PagePanZoom}".WriteInfo();

        //drawing.RenderDrawingSVG(this, tick, fps);
        //Workspace?.RenderWatermark(Ctx, tick);


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
