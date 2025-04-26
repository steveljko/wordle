using System;
using backend.Hubs;
using backend.Models;
using backend.Responses;
using Microsoft.AspNetCore.SignalR;

namespace backend.Services;

public class GameService : IGameService
{
  private readonly ILobbyService _lobbyService;
  private readonly IWordService _wordService;
  private readonly IHubContext<GameHub> _hubContext;
  private Player Drawer { get; set; } = null;
  private string WordToDraw { get; set; } = string.Empty;

  public GameService(ILobbyService lobbyService, IWordService wordService, IHubContext<GameHub> hubContext)
  {
    _lobbyService = lobbyService;
    _wordService = wordService;
    _hubContext = hubContext;
  }

  /// <summary>
  /// Initializes game round by assigning the first player as drawer,
  /// notifying participants of drawer's turn, and setting up the leaderboard.
  /// </summary>
  public async Task OnStart()
  {
    Drawer = _lobbyService.GetAllPlayersInLobby()[0]; // set first user to current drawer
    await NotifyPlayerTurn(Drawer); // notifies next player and allow to choose a word.
    await UpdateLeaderboard(); // initilize leaderboard
  }

  /// <summary>
  /// Selects a word for the game, notifies all connected clients, 
  /// and starts the game timer.
  /// </summary>
  public async Task SelectWord(string word)
  {
    WordToDraw = word;

    await _hubContext.Clients.All.SendAsync("WordSelected");
    await _hubContext.Clients.Client(ActivePlayer.Id).SendAsync("WordToDraw", new { Word = WordToDraw });

    await StartGameTimer(60);
  }

  public async Task<bool> GuessWord(Player player, string word)
  {
    if (_lobbyService.GameIsStarted())
    {
      if (WordToDraw.Equals(word, StringComparison.OrdinalIgnoreCase))
      {
        _lobbyService.AddPointsToPlayer(player.Id, 10);

        await UpdateLeaderboard();

        return true;
      }
    }

    return false;
  }


  public async Task<Player> NextTurn()
  {
    // reset word
    WordToDraw = string.Empty;

    var players = _lobbyService.GetAllPlayersInLobby();
    if (players.Count > 0)
    {
      var currPlayerIdx = _lobbyService.GetAllPlayersInLobby().IndexOf(Drawer);
      var nextPlayerIdx = (currPlayerIdx + 1) % players.Count;

      // change drawer to next player
      Drawer = players[nextPlayerIdx];

      // change user turn
      await NotifyPlayerTurn(Drawer);

      return ActivePlayer;
    }

    return null;
  }

  public async Task UpdateLeaderboard()
  {
    var leaderboard = _lobbyService.GetAllPlayersInLobby()
      .OrderByDescending(p => p.Points)
      .Select(p => new {
        p.Username,
        p.Points
      })
      .ToList();

    await _hubContext.Clients.Groups("Lobby").SendAsync("UpdateLeaderboard", leaderboard);
  }

  /// <summary>
  /// Determines if the player is the current drawer.
  /// </summary>
  public bool IsCurrentDrawer(Player player)
  {
    return player.Id == ActivePlayer.Id;
  }

  // <summary>
  // Notify next user that is his turn and show them words for drawing.
  // </summar>
  private async Task NotifyPlayerTurn(Player player)
  {
    await _hubContext.Clients.Groups("Lobby").SendAsync("Broadcast", new BroadcastResponse
    {
      Message = $"It's {player.Username} turn!",
    });

    var words = _wordService.GetRandomWords(3);
    await _hubContext.Clients.Client(player.Id).SendAsync("YourTurn", new { Words = words });
  }

  /// <summary>
  /// Initiates a game timer for the specified countdown duration. 
  /// After the countdown expires, it clears the game canvas and 
  /// triggers the next turn for the players in the lobby.
  /// </summary>
  private async Task StartGameTimer(int countdown)
  {
    // starts timer and wait
    var endTime = DateTime.UtcNow.AddSeconds(countdown);

    await _hubContext.Clients.Groups("Lobby").SendAsync("StartTimer", new { 
        EndTime = endTime.ToString("o"), // ISO 8601 time format
        Duration = countdown 
        });

    await Task.Delay(countdown * 1000); 

    // clear canvas and switch to next user
    await _hubContext.Clients.Groups("Lobby").SendAsync("ClearCanvas");
    await _hubContext.Clients.Groups("Lobby").SendAsync("ResetTimer");
    await NextTurn();
  }
}
