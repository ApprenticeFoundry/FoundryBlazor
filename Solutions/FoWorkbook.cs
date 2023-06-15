using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Radzen;

namespace FoundryBlazor.Solutions;

public interface IWorkbook
{
    void CreateCommands(IWorkspace space, IJSRuntime js, NavigationManager nav, string serverUrl);
    List<IFoCommand> CollectCommands(List<IFoCommand> list);
    void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav);

    bool SetSignalRHub(HubConnection hub, string panid);
    void PreRender(int tick);
    void PostRender(int tick);
    Task RenderWatermark(Canvas2DContext ctx, int tick);
}

public class FoWorkbook: FoComponent, IWorkbook
{
    protected IWorkspace Workspace { get; set; }
    protected ICommand Command { get; set; }
    protected IFoundryService Foundry { get; set; }
    protected DialogService Dialog { get; set; }
    protected IJSRuntime JsRuntime { get; set; }
    
    protected ComponentBus PubSub { get; set; }

    public FoWorkbook(IWorkspace space, IFoundryService foundry)
    {
        Workspace = space;
        Foundry = foundry;
        Command = foundry.Command();
        Dialog = foundry.Dialog();
        JsRuntime = foundry.JS();
        PubSub = foundry.PubSub();
    }  


    public virtual void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
    }

    public virtual List<IFoCommand> CollectCommands(List<IFoCommand> list)
    {
        return list;
    }

    public virtual void CreateCommands(IWorkspace space, IJSRuntime js, NavigationManager nav, string serverUrl)
    {
    }

    public virtual bool SetSignalRHub(HubConnection hub, string panid)
    {
        return false;
    }
    public virtual void PreRender(int tick)
    {

    }
    public virtual void PostRender(int tick)
    {

    }
    public virtual async Task RenderWatermark(Canvas2DContext ctx, int tick)
    {
        await Task.CompletedTask;
    }
}
