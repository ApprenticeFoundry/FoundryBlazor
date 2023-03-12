

using FoundryBlazor.Extensions;
using FoundryBlazor.Message;
using FoundryBlazor.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace FoundryBlazor.Shape;

public interface ICommand
{
    object? Create(D2D_Create msg);
    bool Move(D2D_Move msg);
    bool Destroy(D2D_Destroy msg);

    HubConnection? GetHub();
    void SetHub(HubConnection hub, string panid);
    bool HasHub();

    bool StartHub();
    bool StopHub();

    Task<bool> Send(D2D_Base msg);
    ValueTask DisposeAsync();
    bool IsConnected { get; }

    void Save();
    void Restore();
}

public class CommandService : ICommand
{
    private bool IsRunning = false;
    private HubConnection? _DrawingSyncHub;
    private readonly IPageManagement _pageManager;


    public CommandService(
        IPageManagement pm
        )
    {
        _pageManager = pm;
    }

    public bool HasHub()
    {
        return _DrawingSyncHub != null;
    }
    public HubConnection? GetHub()
    {
        return _DrawingSyncHub;
    }
    public void SetHub(HubConnection hub, string panid)
    {
        if (_DrawingSyncHub == null)
        {
            _DrawingSyncHub = hub;
            $"SetHub of Pan {panid} CommandService".WriteLine(ConsoleColor.DarkMagenta);
        }
        else
        {
            _DrawingSyncHub = hub;
            $"Reset Hub of Pan {panid}  CommandService".WriteLine(ConsoleColor.DarkMagenta);
        }
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

    public async Task<bool> Send(D2D_Base msg)
    {
        if (_DrawingSyncHub == null) return false;

        $"Sending {IsRunning} {msg.Topic()}..".WriteNote();

        if (IsRunning)
            await _DrawingSyncHub.SendAsync(msg.Topic(), msg);
        
        $"Sent {IsRunning} {msg.Topic()}..".WriteNote();

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
        //$"Create {create.PayloadType} {create.Payload}".WriteLine(ConsoleColor.Magenta);

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
        var shapes = _pageManager.FindShapes(move.TargetId);
        //$"Move {move.Target} {move.Angle} Shape {shape}".WriteLine(ConsoleColor.Magenta);
        shapes?.ForEach(item => item.MoveTo(move.PinX, move.PinY));
        shapes?.ForEach(item => item.RotateTo(move.Angle));
        return shapes != null;
    }

    public bool Destroy(D2D_Destroy destroy)
    {
        var shapes = _pageManager.ExtractShapes(destroy.TargetId);
        //$"Move {move.Target} {move.Angle} Shape {shape}".WriteLine(ConsoleColor.Magenta);
        return shapes != null;
    }


    public FoPage2D CurrentPage()
    {
        return _pageManager.CurrentPage();
    }



    public void Save()
    {
        var version = VersionInfo.Generate(null, "model", "Model Drawing", "steve@gmail.com");

        var model = new ModelPersist(version);
        model.PersistPage(_pageManager.CurrentPage());


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
        model?.RestorePages(_pageManager);
    }
}