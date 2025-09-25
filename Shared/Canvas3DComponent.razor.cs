using BlazorComponentBus;
using BlazorThreeJS.Events;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Settings;

using FoundryBlazor.PubSub;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;
using System.Reflection.Metadata.Ecma335;

namespace FoundryBlazor.Shared;

public partial class Canvas3DComponent : ComponentBase, IAsyncDisposable
{

    [Inject] public IWorkspace? Workspace { get; set; }
    [Inject] private ComponentBus? PubSub { get; set; }

    [Parameter] public string CanvasStyle { get; set; } = "width:max-content; border:1px solid black;cursor:default";
    [Parameter] public int CanvasWidth { get; set; } = 250;
    [Parameter] public int CanvasHeight { get; set; } = 400;


    [Parameter,EditorRequired] public string? SceneName { get; set; }


    protected ViewerThreeD? Viewer3DReference;

    private Scene3D? Ctx;

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

    protected override void OnInitialized()
    {
        //$"Canvas3DComponentBase {SceneName} OnInitialized".WriteInfo();
    }

    public (bool, Scene3D) GetActiveScene() 
    {
        if (Viewer3DReference == null) 
            return (false, null!);

        var scene = Viewer3DReference.GetActiveScene();
        var found = scene != null;
        return (found, scene!);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //  $"Canvas3DComponentBase {SceneName} OnAfterRenderAsync".WriteInfo();
            var (found, scene) = GetActiveScene();
            if (found)
            {
                Ctx = scene; 
                Ctx?.SetAfterUpdateAction((s,j)=>
                {
                    PubSub?.Publish<RefreshUIEvent>(new RefreshUIEvent("Canvas3DComponentBase"));
                });
            }

            PubSub?.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if ( Ctx == null ) return;
            Ctx?.SetAfterUpdateAction((s,j)=> {});
            Ctx = null;

            //"Canvas3DComponentBase DisposeAsync".WriteInfo();
            PubSub?.UnSubscribeFrom<RefreshUIEvent>(OnRefreshUIEvent);
            GC.SuppressFinalize(this);
            await ValueTask.CompletedTask;
        }
        catch (Exception ex)
        {
            $"Canvas3DComponentBase DisposeAsync Exception {ex.Message}".WriteError();
        }
    }

    public void Render()
    {
        var arena = Workspace?.GetArena();
        if ( arena != null )
            arena.UpdateArena();     
    }

    private void OnRefreshUIEvent(RefreshUIEvent e)
    {
        InvokeAsync(StateHasChanged);
        //$"Canvas3DComponentBase OnRefreshUIEvent StateHasChanged {e.note}".WriteInfo();
    }


}
