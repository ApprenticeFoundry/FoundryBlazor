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
    FoPage2D CurrentPage();
    FoPage2D EstablishCurrentPage(string pagename, string color = "Ivory");
    void CreateCommands(IWorkspace space, IJSRuntime js, NavigationManager nav, string serverUrl);
    List<IFoCommand> CollectCommands(List<IFoCommand> list);
    void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav);
    Dictionary<string, Action> DefaultMenu();
    bool SetSignalRHub(HubConnection hub, string panid);
    void PreRender(int tick);
    Task RefreshRender(int tick);
    void PostRender(int tick);
    Task RenderWatermark(Canvas2DContext ctx, int tick);
}

public class FoWorkbook : FoComponent, IWorkbook
{
    protected IWorkspace Workspace { get; set; }
    protected ICommand Command { get; set; }
    protected IFoundryService FoundryService { get; set; }
    protected DialogService Dialog { get; set; }
    protected IJSRuntime JsRuntime { get; set; }
    protected ComponentBus PubSub { get; set; }

    protected IQRCodeService QRCode { get; set; }

    public bool IsActive { get; set; } = false;
    public bool IsDirty { get; set; } = false;
    private FoPage2D? WorkPage { get; set; }

    public FoWorkbook(IWorkspace space, IFoundryService foundry)
    {
        Workspace = space;
        FoundryService = foundry;
        Command = foundry.Command();
        Dialog = foundry.Dialog();
        JsRuntime = foundry.JS();
        PubSub = foundry.PubSub();
        QRCode = foundry.QRCode();
    }



    public FoPage2D EstablishCurrentPage(string pagename, string color)
    {
        var drawing = Workspace.GetDrawing()!;
        var manager = drawing.Pages();
        WorkPage = manager.FindPage(pagename);
        if (WorkPage == null)
        {
            WorkPage = new FoPage2D(pagename, color);
            manager.AddPage(WorkPage);
        }
        drawing.SetCurrentPage(WorkPage);
        return WorkPage;
    }

    public FoPage2D CurrentPage()
    {
        if (WorkPage != null)
            return WorkPage;

        return EstablishCurrentPage(Name, "Black");
    }

    public virtual void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
    }
    public virtual Dictionary<string, Action> DefaultMenu()
    {
        return new Dictionary<string, Action>();
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

    public virtual Task RefreshRender(int tick)
    {
        return Task.CompletedTask;
    }

    public virtual void PostRender(int tick)
    {
    }
    public virtual async Task RenderWatermark(Canvas2DContext ctx, int tick)
    {
        await Task.CompletedTask;
    }
}
