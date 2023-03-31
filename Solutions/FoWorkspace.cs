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
    string GetCurrentUrl();
    string SetCurrentUrl(string url);
    
    Task InitializedAsync(string defaultHubURI);
    IDrawing GetDrawing();
    IArena GetArena();

    string GetUserID();
    ViewStyle GetViewStyle();
    void SetViewStyle(ViewStyle style);
    bool IsViewStyle2D();
    bool IsViewStyle3D();

    FoCommand2D EstablishCommand<T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton2D;


    T EstablishWorkPiece<T>() where T : FoWorkPiece;

    Task DropFileCreateShape(IBrowserFile file, CanvasMouseArgs args);

    void PreRender(int tick);
    void PostRender(int tick);

    Task RenderWatermark(Canvas2DContext ctx, int tick);

    ComponentBus GetPubSub();

}

public class FoWorkspace : FoComponent, IWorkspace
{
    public static bool RefreshCommands { get; set; } = true;
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
    }

    public virtual void PostRender(int tick)
    {
    }



    public virtual async Task RenderWatermark(Canvas2DContext ctx, int tick)
    {
        await Task.CompletedTask;
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
            $"Starting SignalR Hub:{defaultHubURI}".WriteWarning();
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

    
    public string GetCurrentUrl()
    {
        return CurrentUrl;
    }

    public string SetCurrentUrl(string url)
    {
        CurrentUrl = url;
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

    public List<FoWorkPiece> AddWorkPiece(FoWorkPiece piece)
    {
        Add<FoWorkPiece>(piece);
        return Members<FoWorkPiece>();
    }

    public T EstablishWorkPiece<T>() where T : FoWorkPiece
    {
        var piece = Activator.CreateInstance(typeof(T), this, Command, Dialog, JsRuntime) as T;
        AddWorkPiece(piece!);
        return piece!;
    }

    public List<IFoMenu> CollectMenus(List<IFoMenu> list)
    {
        GetMembers<FoMenu2D>()?.ForEach(item => list.Add(item));
        GetMembers<FoMenu3D>()?.ForEach(item => list.Add(item));

        //if (!IsViewStyle3D())
            GetDrawing()?.CollectMenus(list);

        //if (!IsViewStyle2D())
            GetArena()?.CollectMenus(list);

        Members<FoWorkPiece>().ForEach(item => item.CollectMenus(list));

        return list;
    }



    public virtual void CreateMenus(IJSRuntime js, NavigationManager nav)
    {
        var OpenNew = async () =>
        {
            var target = nav!.ToAbsoluteUri("/");
            try
            {
                await js.InvokeAsync<object>("open", target); //, "_blank", "height=600,width=1200");
            }
            catch { }
        };

        GetDrawing()?.EstablishMenu<FoMenu2D>("Main", new Dictionary<string, Action>()
        {
            { "New Window", () => OpenNew()},
            { "View 2D", () => PubSub.Publish<ViewStyle>(ViewStyle.View2D)},
            { "View 3D", () => PubSub.Publish<ViewStyle>(ViewStyle.View3D)},
            { "Pan Zoom", () => GetDrawing()?.TogglePanZoomWindow()},
          //  { "View None", () => PubSub.Publish<ViewStyle>(ViewStyle.None)},
            { "Save Drawing", () => Command.Save()},
            { "Restore Drawing", () => Command.Restore()},
        }, true);

        Members<FoWorkPiece>().ForEach(item => item.CreateMenus(js, nav));

        GetDrawing()?.CreateMenus(js, nav);
        GetArena()?.CreateMenus(js, nav);

    }


    public FoCommand2D EstablishCommand<T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton2D
    {
        var commandBar = Find<FoCommand2D>(name);
        if (commandBar == null)
        {
            commandBar = Activator.CreateInstance(typeof(FoCommand2D), name) as FoCommand2D;
            this.Add<FoCommand2D>(commandBar!);
        }
        if (clear)
            commandBar?.Clear();

        foreach (KeyValuePair<string, Action> item in actions)
        {
            if (Activator.CreateInstance(typeof(T), item.Key, item.Value) is T shape)
                commandBar?.Add<T>(shape);
        }

        return commandBar!;
    }

    public virtual void CreateCommands(IJSRuntime js, NavigationManager nav, string serverUrl)
    {

        var OpenDTAR = async () =>
        {
            var target = nav!.ToAbsoluteUri(serverUrl);
            try
            {
                await js!.InvokeAsync<object>("open", target);
            }
            catch { }
        };


        EstablishCommand<FoButton2D>("CMD", new Dictionary<string, Action>()
        {
            { serverUrl, () => OpenDTAR() },
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


        FoWorkspace.RefreshCommands = true;
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
        FoPage2D.RefreshMenus = true;
        return GetViewStyle() == ViewStyle.View2D;
    }
    public bool IsViewStyle3D()
    {
        FoPage2D.RefreshMenus = true;
        return GetViewStyle() == ViewStyle.View3D;
    }

    public HubConnection EstablishDrawingSyncHub(string defaultHubURI)
    {
        if (Command.HasHub()) return Command.GetSignalRHub()!;

        var secureHub = defaultHubURI.Replace("http://", "https://");
        var secureHubURI = new Uri(secureHub);


        var hub = new HubConnectionBuilder()
            .WithUrl(secureHubURI)
            .Build();

        Command.SetSignalRHub(hub, GetUserID(), Toast);
        SetSignalRHub(hub, GetUserID());


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
