

using BlazorComponentBus;
using FoundryBlazor.Canvas;
using FoundryBlazor.Extensions;
using FoundryBlazor.Message;
using FoundryBlazor.Persistence;
using FoundryBlazor.Shared;
using FoundryBlazor.Solutions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace FoundryBlazor.Shape;

public interface ICommand
{
    IDrawing GetDrawing();
    IArena GetArena();

    D2D_UserToast SendToast(ToastType type, string message);
    D2D_Base SendSyncMessage(D2D_Base msg);
    //D2D_UserMove SendUserMove(CanvasMouseArgs args, bool isActive);
    D2D_Move SendShapeMoved<T>(T shape) where T : FoGlyph2D;
    D2D_Destroy SendShapeDestroy<T>(T shape) where T : FoGlyph2D;
    D2D_Create SendShapeCreate<T>(T shape) where T : FoGlyph2D;
    object? Create(D2D_Create msg);
    bool Move(D2D_Move msg);
    bool Destroy(D2D_Destroy msg);

    HubConnection? GetHub();
    bool SetHub(HubConnection hub, string panid, IToast toast);
    bool HasHub();

    bool StartHub();
    bool StopHub();

    //Task<bool> Send(D2D_Base msg);
    ValueTask DisposeAsync();
    bool IsConnected { get; }


    void Save();
    void Restore();
}

public class CommandService : ICommand
{
    private string UserID { get; set; } = "NO User";
    private bool IsRunning = false;
    private HubConnection? _DrawingSyncHub;
    protected IDrawing ActiveDrawing { get; init; }
    protected IArena ActiveArena { get; init; }
    private D2D_UserMove? UserLocation { get; set; }
    protected ComponentBus PubSub { get; set; }

    public CommandService(
        IDrawing drawing,
        IArena arena,
        ComponentBus pubsub
        )
    {
        ActiveDrawing = drawing;
        ActiveArena = arena;
        PubSub = pubsub;
    }

    public IDrawing GetDrawing()
    {
        return ActiveDrawing;
    }

    public IArena GetArena()
    {
        return ActiveArena;
    }

    public bool HasHub()
    {
        return _DrawingSyncHub != null;
    }
    public HubConnection? GetHub()
    {
        return _DrawingSyncHub;
    }
    public bool SetHub(HubConnection hub, string userid, IToast toast)
    {
        if (_DrawingSyncHub != null)
        {
            $"tried to Reset Hub of Pan {userid}  CommandService".WriteError();
            return false;
        }

        UserID = userid;
        GetDrawing()?.SetUserID(UserID);
        _DrawingSyncHub = hub;
        $"SetHub of Pan {UserID} CommandService".WriteNote();

        PubSub.SubscribeTo<D2D_UserToast>(usertoast =>
        {
            toast.RenderToast(usertoast);
            SendSyncMessage(usertoast);
        });

        PubSub.SubscribeTo<CanvasMouseArgs>(args =>
        {
            SendUserMove(args, true);;
        });

        PubSub.SubscribeTo<FoGlyph2D>(args =>
        {
            SendShapeMoved<FoGlyph2D>(args);
        });

        hub.On<D2D_Create>("Create", (create) =>
         {
             "Received Create".WriteNote();
             var newShape = Create(create);
             if (newShape != null)
                 GetDrawing().AddShape<FoShape2D>((FoShape2D)newShape);
         });

        hub.On<D2D_Move>("Move", (move) =>
        {
            // if (move.PayloadType.Matches("Boid"))
            // {
            //     // $"Receive D2D_Move {move.TargetId} {move.PayloadType} {move.PanID} Message Receive on {PanID}".WriteLine(ConsoleColor.Yellow);
            //     //BoidSimulation?.DoMovement(move);
            //     return;
            // }
            Move(move);
        });

        hub.On<D2D_Destroy>("Destroy", (destroy) =>
        {

            // if (destroy.PayloadType.Matches("Boid"))
            // {
            //     // $"Receive D2D_Move {move.TargetId} {move.PayloadType} {move.PanID} Message Receive on {PanID}".WriteLine(ConsoleColor.Yellow);
            //     // BoidSimulation?.DoDestroy(destroy);
            //     return;
            // }

            Destroy(destroy);

        });

        hub.On<D2D_UserMove>("UserMove", (usermove) =>
        {
            var MyUser = GetDrawing().UpdateOtherUsers(usermove, toast);
        });

        hub.On<D2D_UserToast>("UserToast", (usertoast) =>
        {
            toast.RenderToast(usertoast);
        });

        return true;
    }
    public bool StartHub()
    {
        if (!IsRunning)
        {
            IsRunning = true;
            Task.Run(async () => await _DrawingSyncHub!.StartAsync());
            $"StartHub {IsRunning}..".WriteNote();
        }
        return IsRunning;
    }
    public bool StopHub()
    {
        if (IsRunning)
        {
            IsRunning = false;
            Task.Run(async () => await _DrawingSyncHub!.StopAsync());
            $"StopHub {IsRunning}..".WriteNote();
        }
        return IsRunning;
    }
    public bool IsConnected => _DrawingSyncHub?.State == HubConnectionState.Connected;



    public D2D_UserToast SendToast(ToastType type, string message)
    {
        var toast = new D2D_UserToast();
        toast = (type) switch
        {
            ToastType.Info => toast.Info(message),
            ToastType.Warning => toast.Warning(message),
            ToastType.Error => toast.Error(message),
            ToastType.Success => toast.Success(message),
            ToastType.Note => toast.Note(message),
            _ => toast,
        };
        PubSub.Publish<D2D_UserToast>(toast);
        return toast;
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

    public D2D_Create SendShapeCreate<T>(T shape) where T : FoGlyph2D
    {
        var create = new D2D_Create()
        {
            TargetId = shape.GlyphId,
            Payload = StorageHelpers.Dehydrate<T>(shape, false),
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

    public D2D_Base SendSyncMessage(D2D_Base msg)
    {
        Task.Run(async () =>
        {
            msg.PanID = UserID;
            await Send(msg);
        });
        return msg;
    }
    public async Task<bool> Send(D2D_Base msg)
    {
        if (_DrawingSyncHub == null || !IsRunning)
            $"Command Service {IsRunning} {msg.Topic()}..  is NOT Sending".WriteNote();

        if (_DrawingSyncHub == null) return false;

        //$"Sending {IsRunning} {msg.Topic()}..".WriteNote();

        if (IsRunning)
            await _DrawingSyncHub.SendAsync(msg.Topic(), msg);

        //$"Sent {IsRunning} {msg.Topic()}..".WriteNote();

        return IsRunning;
    }

    public async ValueTask DisposeAsync()
    {
        if (_DrawingSyncHub is not null)
        {
            await _DrawingSyncHub.DisposeAsync();
        }
        _DrawingSyncHub = null;
    }



    public object? Create(D2D_Create create)
    {
        $"Create {create.PayloadType} {create.Payload}".WriteNote();

        if (create.PayloadType.Matches("FoShape2D"))
            return StorageHelpers.Hydrate<FoShape2D>(create.Payload, false);

        else if (create.PayloadType.Matches("FoShape1D"))
            return StorageHelpers.Hydrate<FoShape1D>(create.Payload, false);

        else if (create.PayloadType.Matches("FoText2D"))
            return StorageHelpers.Hydrate<FoText2D>(create.Payload, false);

        // else if (create.PayloadType.Matches("Boid"))
        //     return StorageHelpers.Hydrate<Boid>(create.Payload, false);

        return null;
    }

    public bool Move(D2D_Move move)
    {
        var shapes = Pages().FindShapes(move.TargetId);
        //$"Move {move.Target} {move.Angle} Shape {shape}".WriteLine(ConsoleColor.Magenta);
        shapes?.ForEach(item => item.MoveTo(move.PinX, move.PinY));
        shapes?.ForEach(item => item.RotateTo(move.Angle));
        return shapes != null;
    }

    public bool Destroy(D2D_Destroy destroy)
    {
        var shapes = Pages().ExtractShapes(destroy.TargetId);
        //$"Move {move.Target} {move.Angle} Shape {shape}".WriteLine(ConsoleColor.Magenta);
        return shapes != null;
    }


    public FoPage2D CurrentPage()
    {
        return GetDrawing().CurrentPage();
    }
    public IPageManagement Pages()
    {
        return GetDrawing().Pages();
    }


    public void Save()
    {
        var version = VersionInfo.Generate(null, "model", "Model Drawing", "steve@gmail.com");

        var model = new ModelPersist(version);
        model.PersistPage(CurrentPage());


        var json = StorageHelpers.Dehydrate<ModelPersist>(model, false);
        StorageHelpers.WriteData("storage", version.Filename, json);

        var filename = version.Filename.Replace("json", "zip");
        var compress = json.Compress();
        StorageHelpers.WriteData("storage", filename, compress);
    }

    public void Restore()
    {
        var version = VersionInfo.Generate(null, "model", "Model Drawing", "steve@gmail.com");
        var filename = version.Filename.Replace("json", "zip");
        var compress = StorageHelpers.ReadData("storage", filename);
        var json = compress.Decompress();

        //var json = SettingsHelpers.ReadData("storage", version.filename);

        var model = StorageHelpers.Hydrate<ModelPersist>(json, false);
        model?.RestorePages(Pages());
    }
}