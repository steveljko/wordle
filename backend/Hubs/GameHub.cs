using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;

namespace backend.Hubs;

public class GameHub : Hub
{
    private readonly ILobbyService _lobbyService;
    private readonly IGameService _gameService;

    public GameHub(ILobbyService lobbyService, IGameService gameService)
    {
        _lobbyService = lobbyService;
        _gameService = gameService;
    }
    
    public override Task OnConnectedAsync()
    {
        var username = Context.GetHttpContext().Request.Cookies["username"];
        
        if (!string.IsNullOrEmpty(username))
        {
            Groups.AddToGroupAsync(Context.ConnectionId, "Lobby");

            if (_lobbyService.GameIsStarted())
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

    public async Task GuessWord(string word)
    {
        var player = _lobbyService.FindPlayerById(Context.ConnectionId);
        var ok = await _gameService.GuessWord(_lobbyService.FindPlayerById(Context.ConnectionId), word);

        if (ok)
        {
            await Clients.Groups("Lobby").SendAsync("Broadcast", new
            {
                Success = true,
                Message = $"Player {player.Username} correctly guessed word '{word}' and earned 10 points."
            });
            
            await _gameService.UpdateLeaderboard();
        }
        else
        {
            await Clients.Groups("Lobby").SendAsync("Broadcast", new
            {
                Success = false,
                Message = $"Player {player.Username} failed word guess."
            });
        }
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