using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Radzen;

namespace FoundryBlazor.Solutions;

public interface IWorkPiece
{
    void CreateCommands(IWorkspace space, IJSRuntime js, NavigationManager nav, string serverUrl);
    List<IFoCommand> CollectCommands(List<IFoCommand> list);
    void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav);
    List<IFoMenu> CollectMenus(List<IFoMenu> list);  
    bool SetSignalRHub(HubConnection hub, string panid);
    void PreRender(int tick);
    void PostRender(int tick);
    Task RenderWatermark(Canvas2DContext ctx, int tick);
}

public class FoWorkPiece: FoComponent, IWorkPiece
{
    protected IWorkspace Workspace { get; set; }
    protected ICommand Command { get; set; }
    protected DialogService Dialog { get; set; }
    protected IJSRuntime JsRuntime { get; set; }

    public FoWorkPiece(IWorkspace space, ICommand command, DialogService dialog, IJSRuntime js)
    {
        Workspace = space;
        Command = command;
        Dialog = dialog;
        JsRuntime = js;
    }  

    public virtual List<IFoMenu> CollectMenus(List<IFoMenu> list)
    {
        return list;
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
