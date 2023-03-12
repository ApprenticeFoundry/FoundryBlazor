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



public interface IWorkspace: IWorkPiece
{
    Task InitializedAsync(string defaultHubURI);
    IDrawing? GetDrawing();
    IArena? GetArena();
    D2D_UserToast GetUserToast();
    D2D_UserToast SendToast(D2D_UserToast toast);
    D2D_UserMove SendUserMove(CanvasMouseArgs args, bool isActive);
    D2D_Move SendShapeMoved<T>(T shape) where T : FoGlyph2D;
    D2D_Destroy SendShapeDestroy<T>(T shape) where T : FoGlyph2D;
    D2D_Create SendShapeCreate<T>(T? shape) where T : FoGlyph2D;

    string GetPanID();
    ViewStyle GetViewStyle();
    void SetViewStyle(ViewStyle style);
    bool IsViewStyle2D();
    bool IsViewStyle3D();

    FoCommand2D EstablishCommand<T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton2D;

    T EstablishMenu<T>(string name, Dictionary<string, Action> menu, bool clear) where T : FoMenu2D;

    T EstablishWorkPiece<T>() where T : FoWorkPiece;

    Task DropFileCreateShape(IBrowserFile file, CanvasMouseArgs args);

    void PreRender(int tick);
    void PostRender(int tick);

    Task RenderWatermark(Canvas2DContext ctx, int tick);

}

public class FoWorkspace : FoComponent, IWorkspace
{
    public static bool RefreshCommands { get; set; } = true;
    public string PanID { get; set; } = "";
    protected ViewStyle viewStyle = ViewStyle.View2D;
    public InputStyle InputStyle { get; set; } = InputStyle.Drawing;

    public D2D_UserToast UserToast = new();
    public D2D_UserMove? UserLocation { get; set; }
    public Dictionary<string, D2D_UserMove> OtherUserLocations { get; set; } = new();

    protected IDrawing? ActiveDrawing { get; init; }
    protected IArena? ActiveArena { get; init; }
    public ICommand Command { get; set; }
    public IPanZoomService PanZoom { get; set; }


    protected IToast Toast { get; set; }
    protected ComponentBus PubSub { get; set; }
    protected DialogService Dialog { get; set; }
    protected IJSRuntime JsRuntime { get; set; }

    public Func<IBrowserFile, CanvasMouseArgs, Task> OnFileDrop { get; set; } = async (IBrowserFile file, CanvasMouseArgs args) => { await Task.CompletedTask; };


    public FoWorkspace (
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
    }

    public virtual void PreRender(int tick)
    {
    }

    public virtual void PostRender(int tick)
    {
    }

    public D2D_UserToast GetUserToast()
    {
        return UserToast;
    }

    public virtual async Task RenderWatermark(Canvas2DContext ctx, int tick)
    {
        await Task.CompletedTask;
    }

    public virtual async Task DropFileCreateShape(IBrowserFile file, CanvasMouseArgs args)
    {
        await OnFileDrop.Invoke(file, args);
        await Task.CompletedTask;
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

    public string GetPanID()
    {
        return PanID;
    }

    public IDrawing? GetDrawing()
    {
        return ActiveDrawing;
    }

    public IArena? GetArena()
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
        var piece = Activator.CreateInstance(typeof(T), this, Dialog, JsRuntime) as T;
        AddWorkPiece(piece!);
        return piece!;
    }

    public List<IFoMenu> CollectMenus(List<IFoMenu> list)
    {
        GetMembers<FoMenu2D>()?.ForEach(item => list.Add(item));

        if ( !IsViewStyle3D())
            GetDrawing()?.CollectMenus(list);

        if ( !IsViewStyle2D())
            GetArena()?.CollectMenus(list);

        Members<FoWorkPiece>().ForEach( item => item.CollectMenus(list));

        return list;
    }

    public T EstablishMenu<T>(string name, Dictionary<string, Action> menu, bool clear) where T : FoMenu2D
    {
        var result = ActiveDrawing?.EstablishMenu<T>(name, menu, clear);
        return (result as T)!;
    }

    public virtual void CreateMenus(IJSRuntime js, NavigationManager nav)
    {
        EstablishMenu<FoMenu2D>("View", new Dictionary<string, Action>()
        {
            { "View 2D", () => PubSub.Publish<ViewStyle>(ViewStyle.View2D)},
            { "View 3D", () => PubSub.Publish<ViewStyle>(ViewStyle.View3D)},
            { "View None", () => PubSub.Publish<ViewStyle>(ViewStyle.None)},
        }, true);

        Members<FoWorkPiece>().ForEach(item => item.CreateMenus(js,nav));
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

        var SetDrawingStyle = async () =>
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

        var SetFileDropStyle = async () =>
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
        EstablishCommand<FoButton2D>("CMD", new Dictionary<string, Action>()
        {
            { serverUrl, () => OpenDTAR() },
            { "FileDrop", () => SetFileDropStyle()},
            { "Draw", () => SetDrawingStyle()},
            { "1:1", () => PanZoom.Reset()},
            { "Zoom 2.0", () => PanZoom.SetZoom(2.0)},
            { "Zoom 0.5", () => PanZoom.SetZoom(0.5)},
            { "Down", () => ActiveDrawing?.MovePanBy(0,50)},
            { "Right", () => ActiveDrawing?.MovePanBy(50,0)},
            { "Up", () => ActiveDrawing?.MovePanBy(0,-50)},
            { "Left", () => ActiveDrawing?.MovePanBy(-50,0)},
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
        if (Command.HasHub()) return Command.GetHub()!;

        var secureHub = defaultHubURI.Replace("http://", "https://");
        var secureHubURI = new Uri(secureHub);


        var hub = new HubConnectionBuilder()
            .WithUrl(secureHubURI)
            .Build();

        Command.SetHub(hub, PanID);

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



        hub.On<D2D_Move>("Move", (move) =>
        {
            if (move.PayloadType.Matches("Boid"))
            {
                // $"Receive D2D_Move {move.TargetId} {move.PayloadType} {move.PanID} Message Receive on {PanID}".WriteLine(ConsoleColor.Yellow);
                //BoidSimulation?.DoMovement(move);
                return;
            }
            Command?.Move(move);
        });

        hub.On<D2D_Destroy>("Destroy", (destroy) =>
        {

            if (destroy.PayloadType.Matches("Boid"))
            {
                // $"Receive D2D_Move {move.TargetId} {move.PayloadType} {move.PanID} Message Receive on {PanID}".WriteLine(ConsoleColor.Yellow);
                // BoidSimulation?.DoDestroy(destroy);
                return;
            }

            Command?.Destroy(destroy);

        });

        hub.On<D2D_UserMove>("UserMove", (usermove) =>
        {
            var key = usermove.PanID;
            if (!OtherUserLocations.Remove(key))
                if (usermove.Active)
                    Toast?.Success($"{key} has joined");


            if (usermove.Active)
                OtherUserLocations.Add(key, usermove);
            else
                Toast?.Info($"{key} has left");
        });

        hub.On<D2D_UserToast>("UserToast", (usertoast) =>
        {
            Toast?.RenderToast(usertoast);
        });

        return hub;
    }


    public D2D_UserToast SendToast(D2D_UserToast toast)
    {
        Toast?.RenderToast(toast);
        SendSyncMessage(toast);
        return toast;
    }

    public D2D_Create SendShapeCreate<T>(T? shape) where T : FoGlyph2D
    {
        if ( shape == null) return null;
        var create = new D2D_Create()
        {
            PanID = PanID,
            TargetId = shape.GlyphId,
            PayloadType = shape.GetType().Name
        };
        // $"Send___ D2D_Create {create.TargetId} {create.PayloadType} {create.PanID} Message".WriteLine(ConsoleColor.Yellow);

        SendSyncMessage(create);
        return create;
    }

    public D2D_Destroy SendShapeDestroy<T>(T shape) where T : FoGlyph2D
    {
        var destroy = new D2D_Destroy()
        {
            PanID = PanID,
            TargetId = shape.GlyphId,
            PayloadType = shape.GetType().Name
        };
        // $"Send___ D2D_Destroy {destroy.TargetId} {destroy.PayloadType} {destroy.PanID} Message".WriteLine(ConsoleColor.Yellow);

        SendSyncMessage(destroy);
        return destroy;
    }


    public D2D_Move SendShapeMoved<T>(T shape) where T : FoGlyph2D
    {
        var move = new D2D_Move()
        {
            PanID = PanID,
            TargetId = shape.GlyphId,
            PayloadType = shape.GetType().Name,
            PinX = shape.PinX,
            PinY = shape.PinY,
            Angle = shape.Angle
        };
        // $"Send___ D2D_Move {move.TargetId} {move.PayloadType} {move.PanID} Message".WriteLine(ConsoleColor.Yellow);

        SendSyncMessage(move);
        return move;
    }

    public D2D_UserMove SendUserMove(CanvasMouseArgs args, bool isActive)
    {
        UserLocation ??= new D2D_UserMove();
        UserLocation.Active = isActive;
        UserLocation.X = args.OffsetX;
        UserLocation.Y = args.OffsetY;

        //$"Send___ D2D_UserMove  Message".WriteLine(ConsoleColor.Yellow);

        SendSyncMessage(UserLocation);
        return UserLocation;
    }

    protected D2D_Base SendSyncMessage(D2D_Base msg)
    {
        Task.Run(async () =>
        {
            msg.PanID = this.PanID;
            await Command.Send(msg);
        });
        return msg;
    }

    public List<IFoCommand> CollectCommands(List<IFoCommand> list)
    {
        GetMembers<FoCommand2D>()?.ForEach(item => list.Add(item));
        Members<FoWorkPiece>().ForEach(item => item.CollectCommands(list));
        return list;
    }
}
