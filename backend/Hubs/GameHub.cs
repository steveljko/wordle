using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;

namespace backend.Hubs;

public class GameHub : Hub
{
    private readonly LobbyService _lobbyService;

    public GameHub(LobbyService lobbyService)
    {
        _lobbyService = lobbyService;
    }
    
    public override Task OnConnectedAsync()
    {
        var username = Context.GetHttpContext().Request.Cookies["username"];
        
        if (!string.IsNullOrEmpty(username))
        {
            Groups.AddToGroupAsync(Context.ConnectionId, "Lobby");
                
            var player = new Player
            {
                Id = Context.ConnectionId,
                Username = username,
            };
    
            _lobbyService.AddPlayer(player);
            
            Clients.Group("Lobby").SendAsync("UserJoined", new
            {
                Player = player,
                Players = _lobbyService.GetAllPlayersInLobby()
            });
        }

        return base.OnConnectedAsync();
    }
    
    public async Task StartGame()
    {
        if (_lobbyService.GetAllPlayersInLobby().Count >= 2)
        {
            await Clients.Client(Context.ConnectionId).SendAsync("Error", new
            {
                Code = "NoEnoughPlayersInLobby",
                Message = "Not enough players in the lobby."
            });
        }

        await Clients.All.SendAsync("GameStarted");
    }
    
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        _lobbyService.RemovePlayer(Context.ConnectionId);
    
        await Clients.All.SendAsync("UserLeft", new {
            Players = _lobbyService.GetAllPlayersInLobby()
        });
    
        await base.OnDisconnectedAsync(exception);
    }
}