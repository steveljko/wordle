using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.SignalR;

namespace backend.Hubs;

public class GameHub : Hub
{
    private readonly LobbyService _lobbyService;

    public GameHub(LobbyService lobbyService)
    {
        _lobbyService = lobbyService;
    }
    
    public async Task JoinLobby(string username)
    {
        var player = new Player
        {
            Id = Context.ConnectionId,
            Username = username,
        };
        
        _lobbyService.AddPlayer(player);

        await Clients.All.SendAsync("UserJoined", new
        {
            Player = player,
            Players = _lobbyService.GetAllPlayersInLobby()
        });
    }
    
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        _lobbyService.RemovePlayer(Context.ConnectionId);
    
        await Clients.All.SendAsync("UserLeft", new
        {
            Players = _lobbyService.GetAllPlayersInLobby()
        });
    
        await base.OnDisconnectedAsync(exception);
    }
}