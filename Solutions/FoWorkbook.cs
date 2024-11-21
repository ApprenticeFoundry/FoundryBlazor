using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
using FoundryBlazor.Shape;
using FoundryBlazor.Shared;
using Microsoft.AspNetCore.Components;

using Microsoft.JSInterop;
using Radzen;

namespace FoundryBlazor.Solutions;

public interface IWorkbook
{
    FoPage2D CurrentPage();
    FoPage2D EstablishCurrentPage<T>(string pagename, string color = "Ivory") where T : FoPage2D;
    FoStage3D CurrentStage();
    FoStage3D EstablishCurrentStage<T>(string pagename, string color = "Ivory") where T : FoStage3D;

    void CreateCommands(IWorkspace space, IJSRuntime js, NavigationManager nav, string serverUrl);
    List<IFoCommand> CollectCommands(List<IFoCommand> list);
    List<IFoMenu> CollectMenus(List<IFoMenu> list);
    void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav);
    Dictionary<string, Action> DefaultMenu();
    //bool SetSignalRHub(HubConnection hub, string panid);
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
    protected IPopupDialog PopupDialog { get; set; }
    protected IJSRuntime JsRuntime { get; set; }
    protected ComponentBus PubSub { get; set; }

    protected IQRCodeService QRCode { get; set; }

    public bool IsActive { get; set; } = false;
    public bool IsDirty { get; set; } = false;
    private FoPage2D? WorkPage { get; set; }
    private FoStage3D? WorkStage { get; set; }
    public FoWorkbook(IWorkspace space, IFoundryService foundry)
    {
        Workspace = space;
        FoundryService = foundry;
        Command = foundry.Command();
        PopupDialog = foundry.PopupDialog();
        JsRuntime = foundry.JS();
        PubSub = foundry.PubSub();
        QRCode = foundry.QRCode();
    }



    public FoPage2D EstablishCurrentPage<T>(string pagename, string color) where T: FoPage2D
    {
        var drawing = Workspace.GetDrawing()!;
        var manager = drawing.Pages();
        WorkPage = manager.FindPage(pagename);
        if (WorkPage == null)
        {
            WorkPage = (Activator.CreateInstance(typeof(T), pagename, color) as FoPage2D)!;
            manager.AddPage(WorkPage);
        }
        drawing.SetCurrentPage(WorkPage);
        return WorkPage;
    }

    public FoPage2D CurrentPage()
    {
        if (WorkPage != null)
            return WorkPage;

        return EstablishCurrentPage<FoPage2D>(Key, "Black");
    }

    public FoStage3D EstablishCurrentStage<T>(string pagename, string color) where T: FoStage3D
    {
        var arena = Workspace.GetArena()!;
        var manager = arena.Stages();
        WorkStage = manager.FindStage(pagename);
        if (WorkStage == null)
        {
            WorkStage = (Activator.CreateInstance(typeof(T), pagename, color) as FoStage3D)!;
            manager.AddStage(WorkStage);
        }
        arena.SetCurrentStage(WorkStage);
        return WorkStage;
    }

    public FoStage3D CurrentStage()
    {
        if (WorkStage != null)
            return WorkStage;

        return EstablishCurrentStage<FoStage3D>(Key, "Black");
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
    public List<IFoMenu> CollectMenus(List<IFoMenu> list)
    {
        GetMembers<FoMenu2D>()?.ForEach(item => list.Add(item));
        GetMembers<FoMenu3D>()?.ForEach(item => list.Add(item));
        return list;
    }

    public virtual void CreateCommands(IWorkspace space, IJSRuntime js, NavigationManager nav, string serverUrl)
    {
    }

    // public virtual bool SetSignalRHub(HubConnection hub, string panid)
    // {
    //     return false;
    // }
    public virtual void PreRender(int tick)
    {
    }

    public virtual async Task RefreshRender(int tick)
    {
        await Task.CompletedTask;
    }

    public virtual void PostRender(int tick)
    {
    }
    public virtual async Task RenderWatermark(Canvas2DContext ctx, int tick)
    {
        await Task.CompletedTask;
    }
}
