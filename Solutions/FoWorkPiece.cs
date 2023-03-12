using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

namespace FoundryBlazor.Solutions;

public interface IWorkPiece
{
    void CreateCommands(IJSRuntime js, NavigationManager nav, string serverUrl);
    List<IFoCommand> CollectCommands(List<IFoCommand> list);
    void CreateMenus(IJSRuntime js, NavigationManager nav);
    List<IFoMenu> CollectMenus(List<IFoMenu> list);  
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
    public virtual void CreateMenus(IJSRuntime js, NavigationManager nav)
    {
    }

    public virtual List<IFoCommand> CollectCommands(List<IFoCommand> list)
    {
        return list;
    }

    public virtual void CreateCommands(IJSRuntime js, NavigationManager nav, string serverUrl)
    {
    }
}
