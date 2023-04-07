using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
using FoundryBlazor.Canvas;
using FoundryBlazor.Extensions;
using FoundryBlazor.Message;
using FoundryBlazor.Shape;
using FoundryBlazor.Shared;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Radzen;

namespace FoundryBlazor.Solutions;

public enum ViewStyle { None, View2D, View3D }
public enum InputStyle { None, Drawing, FileDrop }



public interface IWorkspace : IWorkPiece
{
    string GetBaseUrl();
    string SetBaseUrl(string url);
    
    Task InitializedAsync(string defaultHubURI);
    IDrawing GetDrawing();
    IArena GetArena();

    string GetUserID();
    ViewStyle GetViewStyle();
    void SetViewStyle(ViewStyle style);
    bool IsViewStyle2D();
    bool IsViewStyle3D();

    U EstablishCommand<U, T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton2D where U : FoCommand2D;
    U EstablishMenu2D<U, T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton2D where U : FoMenu2D;
    U EstablishMenu3D<U, T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton3D where U : FoMenu3D;

    List<IFoMenu> CollectMenus(List<IFoMenu> list);
    void ClearAllWorkPieces();
    List<FoWorkPiece> AddWorkPiece(FoWorkPiece piece);
    T EstablishWorkPiece<T>() where T : FoWorkPiece;

    Task DropFileCreateShape(IBrowserFile file, CanvasMouseArgs args);

    ComponentBus GetPubSub();

}

public class FoWorkspace : FoComponent, IWorkspace
{
    public static bool RefreshCommands { get; set; } = true;
    public static bool RefreshMenus { get; set; } = true;

    protected string UserID { get; set; } = "";
    protected string CurrentUrl { get; set; } = "";
    protected ViewStyle viewStyle = ViewStyle.View2D;
    public InputStyle InputStyle { get; set; } = InputStyle.Drawing;


    protected IDrawing ActiveDrawing { get; init; }
    protected IArena ActiveArena { get; init; }
    public ICommand Command { get; set; }
    public IPanZoomService PanZoom { get; set; }


    protected IToast Toast { get; set; }
    protected ComponentBus PubSub { get; set; }
    protected DialogService Dialog { get; set; }
    protected IJSRuntime JsRuntime { get; set; }

    public Func<IBrowserFile, CanvasMouseArgs, Task> OnFileDrop { get; set; } = async (IBrowserFile file, CanvasMouseArgs args) => { await Task.CompletedTask; };


    protected Action SetDrawingStyle { get; set; }
    protected Action SetFileDropStyle { get; set; }

    public FoWorkspace(
        IToast toast,
        ICommand command,
        IPanZoomService panzoom,
        IDrawing drawing,
        IArena arena,
        IJSRuntime js,
        DialogService dialog,
        ComponentBus pubSub
        )
    {

        Toast = toast;
        Command = command;
        ActiveDrawing = drawing;
        ActiveArena = arena;
        PubSub = pubSub;
        PanZoom = panzoom;
        Dialog = dialog;
        JsRuntime = js;

        SetDrawingStyle = async () =>
        {
            try
            {
                await js!.InvokeVoidAsync("CanvasFileInput.HideFileInput");
                InputStyle = InputStyle.Drawing;
                await PubSub!.Publish<InputStyle>(InputStyle);
                "SetDrawingStyle".WriteWarning();
            }
            catch { }
        };

        SetFileDropStyle = async () =>
        {
            try
            {
                await js!.InvokeVoidAsync("CanvasFileInput.ShowFileInput");
                InputStyle = InputStyle.FileDrop;
                await PubSub!.Publish<InputStyle>(InputStyle);
                "SetFileDropStyle".WriteWarning();
            }
            catch { }
        };
    }

    public virtual void PreRender(int tick)
    {
        GetMembers<FoWorkPiece>()?.ForEach(item => item.PreRender(tick));
    }

    public virtual void PostRender(int tick)
    {
        GetMembers<FoWorkPiece>()?.ForEach(item => item.PostRender(tick));
    }



    public virtual async Task RenderWatermark(Canvas2DContext ctx, int tick)
    {
        var list = GetMembers<FoWorkPiece>();
        if ( list != null)
            foreach (var item in list)
            {
                await item.RenderWatermark(ctx, tick);
            }
     }

    public virtual async Task DropFileCreateShape(IBrowserFile file, CanvasMouseArgs args)
    {
        await OnFileDrop.Invoke(file, args);
    }

    public async Task InitializedAsync(string defaultHubURI)
    {
        PubSub!.SubscribeTo<ViewStyle>(OnWorkspaceViewStyleChanged);
        if (!Command.HasHub())
        {
            EstablishDrawingSyncHub(defaultHubURI);
            Command.StartHub();
            var note = $"Starting SignalR Hub:{defaultHubURI}".WriteNote();
            //Toast?.Success(note);
            Command.SendToast(ToastType.Info, note);
        }

        await PubSub!.Publish<InputStyle>(InputStyle);
    }

    private void OnWorkspaceViewStyleChanged(ViewStyle e)
    {
        viewStyle = e;
    }

    public string GetUserID()
    {
        if (string.IsNullOrEmpty(UserID))
        {
            var data = new MockDataMaker();
            UserID = data.GenerateName();
        }
        return UserID;
    }

    
    public string GetBaseUrl()
    {
        return CurrentUrl;
    }

    public string SetBaseUrl(string url)
    {
        CurrentUrl = url;
        $"CurrentUrl: {CurrentUrl}".WriteSuccess();
        return CurrentUrl;
    }

    public IDrawing GetDrawing()
    {
        return ActiveDrawing;
    }

    public IArena GetArena()
    {
        return ActiveArena;
    }

    public void ClearAllWorkPieces()
    {
        GetSlot<FoWorkPiece>()?.Clear();
        GetSlot<FoMenu2D>()?.Clear();
        GetSlot<FoMenu3D>()?.Clear();
        FoWorkspace.RefreshCommands = true;
        FoWorkspace.RefreshMenus = true;
        "ClearAllWorkPieces".WriteWarning();
    }

    public List<FoWorkPiece> AddWorkPiece(FoWorkPiece piece)
    {
        Add<FoWorkPiece>(piece);
        FoWorkspace.RefreshCommands = true;
        FoWorkspace.RefreshMenus = true;
        return Members<FoWorkPiece>();
    }

    public T EstablishWorkPiece<T>() where T : FoWorkPiece
    {
        var piece = Activator.CreateInstance(typeof(T), this, Command, Dialog, JsRuntime, PubSub) as T;
        AddWorkPiece(piece!);
        return piece!;
    }

    public List<IFoMenu> CollectMenus(List<IFoMenu> list)
    {
        GetMembers<FoMenu2D>()?.ForEach(item => list.Add(item));
        GetMembers<FoMenu3D>()?.ForEach(item => list.Add(item));

        return list;
    }

   public U EstablishMenu2D<U>(string name, bool clear) where U : FoMenu2D
    {
        var menu = Find<U>(name);
        if (menu == null)
        {
            RefreshMenus = true;
            menu = Activator.CreateInstance(typeof(U), name) as U;
            Add<U>(menu!);
        }
        if (clear)
            menu?.Clear();

        return menu!;
    }

    public U EstablishMenu2D<U, T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton2D where U : FoMenu2D
    {
        var menu = EstablishMenu2D<U>(name,clear);

        foreach (KeyValuePair<string, Action> item in actions)
        {
            if (Activator.CreateInstance(typeof(T), item.Key, item.Value) is T shape)
                menu.Add<T>(shape);
        }

        //menu.LayoutHorizontal();

        return menu;
    }

    public U EstablishMenu3D<U>(string name, bool clear) where U : FoMenu3D
    {
        var menu = Find<U>(name);
        if (menu == null)
        {
            RefreshMenus = true;
            menu = Activator.CreateInstance(typeof(U), name) as U;
            Add<U>(menu!);
        }
        if (clear)
            menu?.Clear();

        return menu!;
    }

    public U EstablishMenu3D<U, T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton3D where U : FoMenu3D
    {
        var menu = EstablishMenu3D<U>(name,clear);

        foreach (KeyValuePair<string, Action> item in actions)
        {
            if (Activator.CreateInstance(typeof(T), item.Key, item.Value) is T shape)
                menu.Add<T>(shape);
        }

        //menu.LayoutHorizontal();

        return menu;
    }

    public virtual void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
        GetSlot<FoMenu2D>()?.Clear();
        GetSlot<FoMenu3D>()?.Clear();

        "FoWorkspace CreateMenus".WriteWarning();
        var OpenNew = async () =>
        {
            var target = nav!.ToAbsoluteUri("/");
            try
            {
                await js.InvokeAsync<object>("open", target); //, "_blank", "height=600,width=1200");
            }
            catch { }
        };

        space.EstablishMenu2D<FoMenu2D,FoButton2D>("Main", new Dictionary<string, Action>()
         {
             { "New Window", () => OpenNew()},
             { "View 2D", () => PubSub.Publish<ViewStyle>(ViewStyle.View2D)},
             { "View 3D", () => PubSub.Publish<ViewStyle>(ViewStyle.View3D)},
             { "Pan Zoom", () => GetDrawing()?.TogglePanZoomWindow()},
           //  { "View None", () => PubSub.Publish<ViewStyle>(ViewStyle.None)},
             { "Save Drawing", () => Command.Save()},
             { "Restore Drawing", () => Command.Restore()},
         }, true);

        GetMembers<FoWorkPiece>()?.ForEach(item => item.CreateMenus(space, js, nav));

        GetDrawing()?.CreateMenus(space,js, nav);
        GetArena()?.CreateMenus(space,js, nav);
    }


    public U EstablishCommand<U>(string name, bool clear) where U : FoCommand2D
    {
        var menu = Find<U>(name);
        if (menu == null)
        {
            RefreshCommands = true;
            menu = Activator.CreateInstance(typeof(U), name) as U;
            Add<U>(menu!);
        }
        if (clear)
            menu?.Clear();

        return menu!;
    }

    public U EstablishCommand<U,T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton2D where U: FoCommand2D
    {
        var commandBar = EstablishCommand<U>(name,clear);

        foreach (KeyValuePair<string, Action> item in actions)
        {
            if (Activator.CreateInstance(typeof(T), item.Key, item.Value) is T shape)
                commandBar?.Add<T>(shape);
        }

        return commandBar!;
    }

    public virtual void CreateCommands(IWorkspace space,  IJSRuntime js, NavigationManager nav, string serverUrl)
    {
        GetSlot<FoCommand2D>()?.Clear();

        var OpenDTAR = async () =>
        {
            var target = nav!.ToAbsoluteUri(serverUrl);
            try
            {
                await js!.InvokeAsync<object>("open", target);
            }
            catch { }
        };


        space.EstablishCommand<FoCommand2D,FoButton2D>("CMD", new Dictionary<string, Action>()
        {
            { "Ping", () => DoPing()},
            { "FileDrop", () => SetFileDropStyle()},
            { "Draw", () => SetDrawingStyle()},
            // { "1:1", () => PanZoom.Reset()},
            // { "Zoom 2.0", () => PanZoom.SetZoom(2.0)},
            // { "Zoom 0.5", () => PanZoom.SetZoom(0.5)},
            // { "Down", () => ActiveDrawing?.MovePanBy(0,50)},
            // { "Right", () => ActiveDrawing?.MovePanBy(50,0)},
            // { "Up", () => ActiveDrawing?.MovePanBy(0,-50)},
            // { "Left", () => ActiveDrawing?.MovePanBy(-50,0)},
            { "Window", () => ActiveDrawing?.TogglePanZoomWindow()},
            { "Hit", () => ActiveDrawing?.ToggleHitTestDisplay()},
        }, true);

        GetMembers<FoWorkPiece>()?.ForEach(item => item.CreateCommands(space,js, nav, serverUrl));

        FoWorkspace.RefreshCommands = true;
    }

    public void DoPing()
    {
        Command.SendToast(ToastType.Info, "Ping");
    }

    public ViewStyle GetViewStyle()
    {
        return viewStyle;
    }
    public void SetViewStyle(ViewStyle view)
    {
        viewStyle = view;
    }
    public bool IsViewStyle2D()
    {
        return GetViewStyle() == ViewStyle.View2D;
    }
    public bool IsViewStyle3D()
    {
        return GetViewStyle() == ViewStyle.View3D;
    }

    public HubConnection EstablishDrawingSyncHub(string defaultHubURI)
    {
        if (Command.HasHub()) 
            return Command.GetSignalRHub()!;

        var secureHub = defaultHubURI.Replace("http://", "https://");
        var secureHubURI = new Uri(secureHub);


        var hub = new HubConnectionBuilder()
            .WithUrl(secureHubURI)
            .Build();

        Command.SetSignalRHub(hub, GetUserID(), Toast);
        SetSignalRHub(hub, GetUserID());

        //Toast?.Success($"HubConnection {secureHubURI} ");

        return hub;
    }

    public bool SetSignalRHub(HubConnection hub, string panid)
    {
        Members<FoWorkPiece>().ForEach(item => item.SetSignalRHub(hub, panid));

        hub.Closed += async (error) =>
       {
           var rand = new Random();
           await Task.Delay(rand.Next(0, 5) * 1000);
           await hub.StartAsync();
       };

        hub.Reconnecting += async (error) =>
        {
            var rand = new Random();
            await Task.Delay(rand.Next(0, 5) * 1000);
        };

        hub.Reconnected += async (error) =>
        {
            var rand = new Random();
            await Task.Delay(rand.Next(0, 5) * 1000);
        };
        return true;
    }


    public List<IFoCommand> CollectCommands(List<IFoCommand> list)
    {
        GetMembers<FoCommand2D>()?.ForEach(item => list.Add(item));
        Members<FoWorkPiece>().ForEach(item => item.CollectCommands(list));
        return list;
    }

    public ComponentBus GetPubSub()
    {
        return PubSub;
    }
}
