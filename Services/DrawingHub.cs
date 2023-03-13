

using FoundryBlazor.Message;
using Microsoft.AspNetCore.SignalR;

namespace FoundryBlazor.Hubs;

public interface IDrawingHub
{
    Task<string> Ping(string payload);
}

public class DrawingSyncHub : Hub, IDrawingHub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
    public async Task<string> Ping(string payload)
    {
        var data = $"DrawingHub: pong message {payload}";
        await Clients.All.SendAsync("Pong", data);
        return data;
    }

    public async Task Create(D2D_Create message)
    {
        await Clients.Others.SendAsync(message.Topic(), message);
    }

    public async Task Move(D2D_Move message)
    {
        await Clients.Others.SendAsync(message.Topic(), message);
    }

    public async Task Destroy(D2D_Destroy message)
    {
        await Clients.Others.SendAsync(message.Topic(), message);
    }

    public async Task Glue(D2D_Glue message)
    {
        await Clients.Others.SendAsync(message.Topic(), message);
    }
    public async Task Unglue(D2D_Unglue message)
    {
        await Clients.Others.SendAsync(message.Topic(), message);
    }
    public async Task UserMove(D2D_UserMove message)
    {
        await Clients.Others.SendAsync(message.Topic(), message);
    }
    public async Task UserToast(D2D_UserToast message)
    {
        await Clients.Others.SendAsync(message.Topic(), message);
    }
}