
using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
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
    void SendShapeCreate(FoGlyph2D? shape);
    void SendGlue(FoGlue2D? glue);

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
            SendShapeMoved(args);
        });

        PubSub.SubscribeTo<D2D_Create>(create =>
        {
            SendSyncMessage(create);
        });
        PubSub.SubscribeTo<D2D_Move>(move =>
        {
            SendSyncMessage(move);
        });

        PubSub.SubscribeTo<D2D_Destroy>(destroy =>
        {
            SendSyncMessage(destroy);
        });

        hub.On<D2D_Create>("Create", (create) =>
         {
             "Received Create".WriteNote();
            UpdateCreate(create);
         });

        hub.On<D2D_Glue>("Glue", (glue) =>
        {
            UpdateGlue(glue);
        });

        hub.On<D2D_Unglue>("Unglue", (glue) =>
        {
            UpdateUnglue(glue);
        });
        hub.On<D2D_Move>("Move", (move) =>
        {
            UpdateMove(move);
        });

        hub.On<D2D_Destroy>("Destroy", (destroy) =>
        {
            UpdateDestroy(destroy);
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

        SendSyncMessage(UserLocation);
        return UserLocation;
    }

    public void SendShapeCreate(FoGlyph2D? shape)
    {
        if ( shape == null) return;
        var create = new D2D_Create(shape);
        SendSyncMessage(create);
    }

    public void SendGlue(FoGlue2D? glue)
    {
        if ( glue == null) return;
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
        if (_DrawingSyncHub == null || !IsRunning)
            $"Command Service {IsRunning} {msg.Topic()}..  is NOT Sending".WriteNote();

        if (_DrawingSyncHub == null) return false;

        //$"Sending {IsRunning} {msg.Topic()}..".WriteNote();

        if (IsRunning)
            await _DrawingSyncHub.SendAsync(msg.Topic(), msg);

        if ( msg is D2D_UserMove) return IsRunning;

        $"Sent {IsRunning} {msg.UserID} {msg.Topic()}..".WriteNote();

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

	public static object? HydrateObject(D2D_Create Source)
    {
        var Payload = Source.Payload;
        var PayloadType = Source.PayloadType;

        var assembly = typeof(D2D_Create).Assembly;
		//var nameSpace = assembly.GetName().Name;
        
		Type type = assembly.DefinedTypes.FirstOrDefault(item => item.Name == PayloadType);
        if ( type == null) return null;

        var node = JsonNode.Parse(Source.Payload);
        if ( node == null) return null;

		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);
		node.WriteTo(writer);
		writer.Flush();

		var options = new JsonSerializerOptions()
		{
			IncludeFields = true,
			IgnoreReadOnlyFields = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};


		var result = JsonSerializer.Deserialize(stream.ToArray(), type, options);

		return result;
	}



    public bool UpdateCreate(D2D_Create create)
    {
        $"Create {create.PayloadType} {create.Payload}".WriteNote();

        var newShape = HydrateObject(create) as FoGlyph2D;

        $"newShape {newShape?.GetType().Name} {newShape?.Name}".WriteNote();
        if (newShape != null)
            GetDrawing().AddShape<FoGlyph2D>(newShape);

       $"UpdateCreate {create.TargetId} {create.PayloadType} {create.UserID}".WriteSuccess();

        return newShape != null;
    }

    public bool UpdateMove(D2D_Move move)
    {
        var shapes = Pages().FindShapes(move.TargetId);

        shapes?.ForEach(item => item.MoveTo(move.PinX, move.PinY));
        shapes?.ForEach(item => item.RotateTo(move.Angle));
        $"UpdateMove {move.TargetId} {move.PayloadType} {move.UserID}".WriteSuccess();
  
        return shapes != null;
    }

    public bool UpdateGlue(D2D_Glue glue)
    { 
        var target = Pages().FindShapes(glue.TargetId).FirstOrDefault();
        var source = Pages().FindShapes(glue.SourceId).FirstOrDefault();
        
        if ( target != null && source is IGlueOwner owner) 
        {
            $"UpdateGlue {glue.TargetId} {glue.SourceId} {glue.PayloadType} {glue.UserID} - {glue.Name}".WriteSuccess();
            var glueObj = new FoGlue2D(glue.Name);
            glueObj.GlueTo(owner, target);

            return true;
        }

        return false;
    }
    public bool UpdateUnglue(D2D_Unglue glue)
    {
        $"UpdateUnglue {glue.TargetId} {glue.SourceId} {glue.PayloadType} {glue.UserID} - {glue.Name}".WriteSuccess();

        var source = Pages().FindShapes(glue.SourceId).FirstOrDefault();
        if ( source is IGlueOwner owner) 
        {
            owner.RemoveGlue(glue.Name);
            return true;
        }


        return false;
    }
    public bool UpdateDestroy(D2D_Destroy destroy)
    {
        var shapes = Pages().ExtractShapes(destroy.TargetId);
        $"UpdateDestroy {destroy.TargetId} {destroy.PayloadType} {destroy.UserID}".WriteSuccess();
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