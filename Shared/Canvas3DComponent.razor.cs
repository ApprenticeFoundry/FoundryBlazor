using BlazorComponentBus;
using BlazorThreeJS.Events;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Settings;
using BlazorThreeJS.Viewers;

 
using FoundryBlazor.PubSub;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;

namespace FoundryBlazor.Shared;

public class Canvas3DComponentBase : ComponentBase, IDisposable, IAsyncDisposable
{

    public Viewer ThreeJSViewer3D = null!;
    private ViewerSettings? Settings { get; set; }
    private Scene? ActiveScene { get; set; }


    [Inject] public IWorkspace? Workspace { get; set; }
    [Inject] private ComponentBus? PubSub { get; set; }

    [Parameter] public string CanvasStyle { get; set; } = "width:max-content; border:1px solid black;cursor:default";
    [Parameter] public int CanvasWidth { get; set; } = 2500;
    [Parameter] public int CanvasHeight { get; set; } = 4000;
    private int tick = 0;


    public ViewerSettings GetSettings()
    {
        Settings ??= new()
        {
            CanSelect = true,// default is false
            SelectedColor = "black",
            WebGLRendererSettings = new WebGLRendererSettings
            {
                Antialias = false // if you need poor quality for some reasons
            }
        };

        return Settings;
    }

    public string GetCanvasStyle()
    {
        var style = new StringBuilder(CanvasStyle).Append("; ").Append("width:").Append(CanvasWidth).Append("px; ").Append("height:").Append(CanvasHeight).Append("px; ").ToString();
        return style;
    }

    public Scene GetActiveScene()
    {
        ActiveScene ??= new Scene();
        return ActiveScene;
    }

    public void Dispose()
    {
        "Canvas3DComponentBase Dispose".WriteInfo();
        ActiveScene = null;
        Settings = null;
         
        ThreeJSViewer3D.ObjectLoaded -= OnThreeJSObjectLoaded;
        PubSub!.UnSubscribeFrom<RefreshUIEvent>(OnRefreshUIEvent);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        "Canvas3DComponentBase DisposeAsync".WriteInfo();
        await ValueTask.CompletedTask;
    }


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var arena = Workspace?.GetArena();
            arena?.SetViewer(ThreeJSViewer3D, GetActiveScene());
            arena?.SetCanvasSizeInPixels(CanvasWidth, CanvasHeight);


            PubSub!.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);
            ThreeJSViewer3D.ObjectLoaded += OnThreeJSObjectLoaded;
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task OnThreeJSObjectLoaded(Object3DArgs e)
    {
        $"OnThreeJSObjectLoaded Returned  {e.UUID}".WriteInfo();
        await Task.CompletedTask;
    }

    private void OnRefreshUIEvent(RefreshUIEvent e)
    {
        //InvokeAsync(StateHasChanged);
        //$"Canvas3DComponentBase OnRefreshUIEvent StateHasChanged {e.note}".WriteInfo();

        Task.Run(async () =>
        {
            await ThreeJSViewer3D.UpdateScene();
            $"after ThreeJSView3D.UpdateScene() {e.note}".WriteInfo();
        });
    }


    public FoPage2D GetCurrentPage()
    {
        return Workspace!.CurrentPage();
    }

    public async Task RenderFrameOBSOLITE(double fps)
    {
        if (ActiveScene == null) 
            return;
        tick++;

        $"Canvas3D RenderFrame {tick} {fps}".WriteInfo();

        Workspace?.PreRender(tick);

        var arena = Workspace?.GetArena();
        if (arena == null) return;

        var stage = arena.CurrentStage();
        if (stage == null) return;

        // $"RenderFrame {tick} {stage.Name} {stage.IsDirty}".WriteError();

        //if you are already rendering then skip it this cycle
        //if (drawing.SetCurrentlyRendering(true)) return;


        await arena.RenderArena(ActiveScene, tick, fps);
        //Workspace?.RenderWatermark(Ctx, tick);


        //drawing.SetCurrentlyRendering(false);

        //Workspace?.PostRender(tick);

        if (stage.IsDirty)
        {
            stage.IsDirty = false;
            await ThreeJSViewer3D.UpdateScene();
            //$"RenderFrame stage.IsDirty  so... ThreeJSView3D.UpdateScene()  {tick} {stage.Name}".WriteSuccess();
        }
    }


}
