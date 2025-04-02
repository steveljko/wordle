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

            if (_lobbyService.lobby.HasGameInProgress == true)
            {
                Clients.Groups("Lobby").SendAsync("GameStarted");
            }
                
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
    
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        _lobbyService.RemovePlayer(Context.ConnectionId);
    
        await Clients.All.SendAsync("UserLeft", new {
            Players = _lobbyService.GetAllPlayersInLobby()
        });
    
        await base.OnDisconnectedAsync(exception);
    }
}