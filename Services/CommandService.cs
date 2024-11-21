using BlazorComponentBus;
 

using FoundryBlazor.Message;
using FoundryBlazor.Persistence;
using FoundryBlazor.Shared;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.SignalR;


namespace FoundryBlazor.Shape;

public interface ICommand
{
    IDrawing GetDrawing();
    IArena GetArena();
    Task Ping(string msg);
    D2D_Base SendSyncMessage(D2D_Base msg);
    D2D_UserToast SendToast(ToastType type, string message);
    void SendShapeCreate(FoGlyph2D? shape);
    void SendGlue(FoGlue2D? glue);

    // HubConnection? GetSignalRHub();
    // bool SetSignalRHub(HubConnection hub, Uri uri, string panid, IToast toast);
    Uri? GetServerUri();
    // bool HasHub();
    // bool StartHub();
    // bool StopHub();
    //bool IsConnected { get; }

    Task<bool> Send(D2D_Base msg);
    ValueTask DisposeAsync();

    void Save();
    bool Restore();
}

public class CommandService : ICommand
{
    private Uri? ServerUri { get; set; }
    private string UserID { get; set; } = "NO User";
    private bool IsRunning = false;
    protected IDrawing ActiveDrawing { get; init; }
    protected IArena ActiveArena { get; init; }
    protected IToast? Toast { get; set; }
    private D2D_UserMove? UserLocation { get; set; }
    protected ComponentBus PubSub { get; set; }

    //private HubConnection? _DrawingSyncHub;

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

    // public bool HasHub()
    // {
    //     return _DrawingSyncHub != null;
    // }
    // public HubConnection? GetSignalRHub()
    // {
    //     return _DrawingSyncHub;
    // }
    public Uri? GetServerUri()
    {
        return ServerUri;
    }
    // public bool SetSignalRHub(HubConnection hub, Uri uri, string userid, IToast toast)
    // {
    //     if (_DrawingSyncHub != null)
    //     {
    //         $"tried to Reset Hub of UserID {userid}  CommandService".WriteError();
    //         return false;
    //     }

    //     _DrawingSyncHub = hub;
    //     ServerUri = uri;
    //     UserID = userid;
    //     GetDrawing()?.SetUserID(UserID);
    //     Toast = toast;
    //     $"SetSignalRHub of UserID {UserID} CommandService".WriteNote();

    //     PubSub.SubscribeTo<D2D_UserToast>(usertoast =>
    //     {
    //         Toast.RenderToast(usertoast);
    //         SendSyncMessage(usertoast);
    //     });

    //     PubSub.SubscribeTo<CanvasMouseArgs>(args =>
    //     {
    //         if (IsRunning)
    //             SendUserMove(args, true); ;
    //     });

    //     PubSub.SubscribeTo<FoGlyph2D>(args =>
    //     {
    //         if (IsRunning)
    //             SendShapeMoved(args);
    //     });

    //     PubSub.SubscribeTo<D2D_Create>(create =>
    //     {
    //         SendSyncMessage(create);
    //     });

    //     PubSub.SubscribeTo<D2D_Move>(move =>
    //     {
    //         SendSyncMessage(move);
    //     });

    //     PubSub.SubscribeTo<D2D_Destroy>(destroy =>
    //     {
    //         SendSyncMessage(destroy);
    //     });


    //     hub.On<string>("Pong", (msg) =>
    //      {
    //          Toast?.Success($"Pong {msg}");
    //      });

    //     hub.On<D2D_Create>("Create", (create) =>
    //      {
    //          UpdateCreate(create);
    //      });

    //     hub.On<D2D_Glue>("Glue", (glue) =>
    //     {
    //         UpdateGlue(glue);
    //     });

    //     hub.On<D2D_Unglue>("Unglue", (glue) =>
    //     {
    //         UpdateUnglue(glue);
    //     });
    //     hub.On<D2D_Move>("Move", (move) =>
    //     {
    //         UpdateMove(move);
    //     });

    //     hub.On<D2D_Destroy>("Destroy", (destroy) =>
    //     {
    //         UpdateDestroy(destroy);
    //     });

    //     hub.On<D2D_UserMove>("UserMove", (usermove) =>
    //     {
    //         var MyUser = GetDrawing().UpdateOtherUsers(usermove, toast);
    //     });

    //     hub.On<D2D_UserToast>("UserToast", (usertoast) =>
    //     {
    //         toast.RenderToast(usertoast);
    //     });

    //     return true;
    // }
    // public bool StartHub()
    // {
    //     if (!IsRunning)
    //     {
    //         IsRunning = true;
    //         Task.Run(async () => await _DrawingSyncHub!.StartAsync());
    //         var note = $"StartHub {IsRunning}..".WriteNote();
    //         Toast?.Success(note);
    //     }
    //     return IsRunning;
    // }
    // public bool StopHub()
    // {
    //     if (IsRunning)
    //     {
    //         IsRunning = false;
    //         Task.Run(async () => await _DrawingSyncHub!.StopAsync());
    //         var note = $"StopHub {IsRunning}..".WriteNote();
    //         Toast?.Error(note);
    //     }
    //     return IsRunning;
    // }
    // public bool IsConnected => _DrawingSyncHub?.State == HubConnectionState.Connected;



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

        SendSyncMessage(UserLocation);
        return UserLocation;
    }

    public void SendShapeCreate(FoGlyph2D? shape)
    {
        if (shape == null) return;
        var create = new D2D_Create(shape);
        SendSyncMessage(create);
    }

    public void SendGlue(FoGlue2D? glue)
    {
        if (glue == null) return;
        var obj = new D2D_Glue(glue);
        SendSyncMessage(obj);
    }

    public void SendShapeDestroy(FoGlyph2D shape)
    {
        var destroy = new D2D_Destroy(shape);
        SendSyncMessage(destroy);
    }


    public void SendShapeMoved(FoGlyph2D shape)
    {
        var move = new D2D_Move(shape);
        SendSyncMessage(move);
    }

    public async Task Ping(string msg)
    {
        await Task.CompletedTask;
        var message = $"Ping {msg} {DateTime.Now}";
        // if (IsRunning && _DrawingSyncHub != null)
        //     await _DrawingSyncHub.SendAsync("Ping", message);
        // else
            $"Command Service {IsRunning} {msg}..  is NOT Sending".WriteNote();

    }

    public D2D_Base SendSyncMessage(D2D_Base msg)
    {
        Task.Run(async () =>
        {
            msg.UserID = UserID;
            await Send(msg);
        });
        return msg;
    }
    public async Task<bool> Send(D2D_Base msg)
    {
        await Task.CompletedTask;
        if (!IsRunning)
            $"Command Service {IsRunning} {msg.Topic()}..  is NOT Sending".WriteNote();

        // if (_DrawingSyncHub == null)
        //     return false;

        //$"Sending {IsRunning} {msg.Topic()}..".WriteNote();

        // if (IsRunning)
        //     await _DrawingSyncHub.SendAsync(msg.Topic(), msg);

        if (msg is D2D_UserMove)
            return IsRunning;

        // $"Sent {IsRunning} {msg.UserID} {msg.Topic()}..".WriteNote();

        return IsRunning;
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
        // if (_DrawingSyncHub is not null)
        // {
        //     await _DrawingSyncHub.DisposeAsync();
        // }
        // _DrawingSyncHub = null;
    }





    public bool UpdateCreate(D2D_Create create)
    {
       // $"Create {create.PayloadType} {create.Payload}".WriteNote();

        var type = StorageHelpers.LookupType(create.PayloadType);
        if (type == null) return false;

        var result = CodingExtensions.HydrateObject(type, create.Payload);
        if (result is not FoGlyph2D newShape) return false;

        //$"newShape {newShape.GetType().Name} {newShape.Name}".WriteNote();

        var drawing = GetDrawing();
        drawing.AddShape<FoGlyph2D>(newShape);

       // $"UpdateCreate {create.TargetId} {create.PayloadType} {create.UserID}".WriteSuccess();
       // $"Must match {newShape.GetGlyphId()} <=> {create.TargetId} ".WriteSuccess();

        return true;
    }

    public bool UpdateMove(D2D_Move move)
    {
        var shapes = Pages().FindShapes(move.TargetId);

        shapes?.ForEach(item => item.MoveTo(move.PinX, move.PinY));
        shapes?.ForEach(item => item.RotateTo(move.Angle));
       // $"UpdateMove {move.TargetId} {move.PayloadType} {move.UserID}".WriteSuccess();

        return shapes != null;
    }

    public bool UpdateGlue(D2D_Glue glue)
    {
        var pages = Pages();
        //$"Glue target {glue.TargetId} target {glue.SourceId}  {glue.Name}".WriteInfo();

        var body = pages.FindShapes(glue.BodyId).FirstOrDefault();
        //var target = pages.FindShapes(glue.TargetId).FirstOrDefault();
        var source = pages.FindShapes(glue.SourceId).FirstOrDefault();

        if (body != null && source is IGlueOwner owner)
        {
            $"UpdateGlue {glue.TargetId} {glue.SourceId} {glue.PayloadType} {glue.UserID} - {glue.Name}".WriteSuccess();
            var child = glue.Name.Split("_")[1];

            var glueObj = new FoGlue2D(glue.Name);
            if ( child.Matches("CENTER") ) 
            {
                glueObj.GlueTo(owner, body, body);
            } 
            else 
            {
                var target = body.FindConnectionPoint(child, true) ?? body;
                glueObj.GlueTo(owner, target, body);
            }

            return true;
        }

        return false;
    }
    public bool UpdateUnglue(D2D_Unglue glue)
    {
        //$"UpdateUnglue {glue.TargetId} {glue.SourceId} {glue.PayloadType} {glue.UserID} - {glue.Name}".WriteSuccess();

        var source = Pages().FindShapes(glue.SourceId).FirstOrDefault();
        if (source is IGlueOwner owner)
        {
            owner.RemoveGlue(glue.Name);
            return true;
        }


        return false;
    }
    public bool UpdateDestroy(D2D_Destroy destroy)
    {
        var shapes = Pages().ExtractShapes(destroy.TargetId);
        //$"UpdateDestroy {destroy.TargetId} {destroy.PayloadType} {destroy.UserID}".WriteSuccess();
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

    public static string LastSavedVersionNumber(string folder, string filename)
    {
        StorageHelpers.EstablishDirectory(folder);

        var found = LastSavedFileVersion(folder, filename);

        if (found != null)
        {
            var version = found.Split("_").Last();
            version = version.Split(".").First();
            return version;
        }
        return "0000";
    }

    public static string? LastSavedFileVersion(string folder, string filename)
    {
        StorageHelpers.EstablishDirectory(folder);

        var root = filename.Split("_").First();
        var ext = filename.Split(".").Last();

        var files = Directory.GetFiles(folder);
        var found = files.Where(item => Path.GetFileName(item).StartsWith(root) && Path.GetFileName(item).EndsWith(ext))
                         .OrderByDescending(item => Path.GetFileName(item))
                         .FirstOrDefault();

        return found;
    }

    public void Save()
    {
        var lastVersion = LastSavedVersionNumber("storage", "model_0000.json");
        //$"lastVersion {lastVersion}".WriteNote();

        var version = VersionPersistence.Generate(lastVersion, "model", "Model Drawing", "steve@gmail.com");

        var model = new ModelPersist(version);
        model.PersistPage(CurrentPage());

        var json = CodingExtensions.Dehydrate<ModelPersist>(model, false);
        //StorageHelpers.WriteData("storage", version.Filename, json);

        var filename = version.Filename.Replace("json", "zip");
        var compress = json.Compress();
        //StorageHelpers.WriteData("storage", filename, compress);
        SendToast(ToastType.Success, $"{version.Filename} is Saved");
    }

    public bool Restore()
    {
        var version = LastSavedFileVersion("storage", "model_0000.json");
        if (version == null) return false;
        var filename = version.Replace("json", "zip");
        var compress = StorageHelpers.ReadData("storage", filename);
        var json = compress.Decompress();

        //var json = SettingsHelpers.ReadData("storage", version.filename);

        var model = CodingExtensions.Hydrate<ModelPersist>(json, false);
        model?.RestorePages(Pages());

        SendToast(ToastType.Success, $"{filename} is Restored");
        return true;
    }
}