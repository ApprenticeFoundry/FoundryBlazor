using BlazorComponentBus;
using Microsoft.AspNetCore.Components;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using FoundryBlazor.Solutions;
using FoundryBlazor.Shape;
using BlazorThreeJS.Viewers;
using FoundryBlazor.PubSub;



namespace FoundryBlazor.Shared;

public partial class ShapeTreeView : ComponentBase, IDisposable
{

    [Inject] private ComponentBus? PubSub { get; set; }
    [Inject] private IFoundryService? Service { get; set; }

    public List<ITreeNode> AllNodes = new();

    private bool _includedScenes = true;
    private bool _includeWorlds = true;

    public IEnumerable<ITreeNode> GetAllNodes()
    {
        if (Service != null)
        {
            var worlds = Service.WorldManager().AllWorlds();

            AllNodes.Clear();

            if ( _includeWorlds && worlds.Count > 0)
            {
                var folder = new FoFolder("Worlds");
                AllNodes.Add(folder);
                worlds.ForEach(item => folder.AddChild(item));
            }

            AllNodes.Add(Service.Drawing());
            AllNodes.Add(Service.Arena());


            if ( _includedScenes )
            {
                var (found, scene) = Service.Arena().CurrentScene();
                if (found)
                {
                    var folder = new FoFolder("Scenes");
                    AllNodes.Add(folder);
                    folder.AddTreeNode(scene!);
                }
            }

            
        }

        return AllNodes;
    }



//https://learn.microsoft.com/en-us/semantic-kernel/overview/

    public void OnRefresh()
    {
        PubSub?.Publish<RefreshUIEvent>(new RefreshUIEvent("ShapeTree"));
    }

    protected void Refresh()
    {
        //var nodes = GetAllNodes().Where( n => n != null).ToList();

        GetAllNodes().ForEach(n => n?.SetExpanded(true));
        Task.Run(() => {
            Thread.Sleep(200);
            //KnBase.RefreshTree = true;
            //$"ShapeTreeBase Refresh".WriteInfo();
            InvokeAsync(StateHasChanged);
        });
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Refresh();
            PubSub?.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private void OnRefreshUIEvent(RefreshUIEvent e)
    {
        Refresh();
    }
    
    public void Dispose()
    {
        PubSub?.UnSubscribeFrom<RefreshUIEvent>(OnRefreshUIEvent);
        GC.SuppressFinalize(this);
    }
}