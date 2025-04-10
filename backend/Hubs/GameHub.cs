using backend.Models;
using backend.Services;
using backend.Responses;
using Microsoft.AspNetCore.SignalR;

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

    if (_gameService.IsCurrentDrawer(player))
    {
      await Clients.Caller.SendAsync("Broadcast", new BroadcastResponse
          {
          Success = false,
          Message = "You can't guess the word while you're drawing!"
          });

      return;
    }

    var ok = await _gameService.GuessWord(player, word);

    if (ok)
    {
      await Clients.Groups("Lobby").SendAsync("Broadcast", new BroadcastResponse
          {
          Message = $"Player {player.Username} correctly guessed word '{word}' and earned 10 points.",
          });

      await _gameService.NextTurn();
    }
    else
    {
      await Clients.Groups("Lobby").SendAsync("Broadcast", new BroadcastResponse
          {
          Success = false,
          Message = $"Player {player.Username} failed word guess."
          });
    }
  }

  public override async Task OnDisconnectedAsync(Exception exception)
  {
    var disconnectedPlayer = _lobbyService.FindPlayerById(Context.ConnectionId);
    bool wasDrawer = _gameService.IsCurrentDrawer(disconnectedPlayer);

    _lobbyService.RemovePlayer(Context.ConnectionId);
    var remainingPlayers = _lobbyService.GetAllPlayersInLobby();

    if (_lobbyService.GetAllPlayersInLobby().Count < 2)
    {
      _lobbyService.StopGame();
      await Clients.All.SendAsync("GameDone");
    } else if (wasDrawer && _lobbyService.GameIsStarted())
    {
      var nextDrawer = _gameService.NextTurn();
    }

    await Clients.All.SendAsync("UserLeft", new {
        Players = _lobbyService.GetAllPlayersInLobby()
        });

    await base.OnDisconnectedAsync(exception);
  }
}
