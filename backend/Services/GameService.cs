using System;
using backend.Hubs;
using backend.Models;
using backend.Responses;
using Microsoft.AspNetCore.SignalR;

namespace backend.Services;

public class GameService : IGameService
{
    private readonly ILobbyService _lobbyService;
    private readonly IHubContext<GameHub> _hubContext;
    private int CurrentPlayerIdx { get; set; } = 0;
    private string WordToDraw { get; set; } = "asd123";
    private static readonly List<string> _wordList = new List<string>
    {
        "Apple", "House", "Tree", "Sun", "Cat", "Dog", "Flower", "Book", "Star", "Moon",
        "Ball", "Hat", "Car", "Boat", "Fish", "Bird", "Cloud", "Chair", "Cup", "Candle",
        "Heart", "Pencil", "Clock", "Carrot", "Umbrella", "Sock", "Butterfly", "Turtle", "Rainbow", "Cake",
        "Leaf", "Spider", "Ghost", "Snowman", "Balloon", "Rocket", "Pizza", "Ice cream", "Crown", "Robot",
        "Dragon", "Frog", "Penguin", "Guitar", "Elephant", "Octopus", "Cupcake", "Rainbow", "Snail", "Unicorn"
    };
    
    public GameService(ILobbyService lobbyService, IHubContext<GameHub> hubContext)
    {
        _lobbyService = lobbyService;
        _hubContext = hubContext;
    }

    public async Task OnStart()
    {
      await UpdateLeaderboard(); // initilize leaderboard
      await NotifyNextPlayer(); // notifies next player and allow to choose a word.
    }

    public async Task SelectWord(string word)
    {
      WordToDraw = word;

      await _hubContext.Clients.All.SendAsync("WordSelected");

      await StartGameTimer(10);
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
            CurrentPlayerIdx = (CurrentPlayerIdx + 1) % players.Count;
            
            var nextDrawer = players[CurrentPlayerIdx];

            await NotifyNextPlayer();
            
            return nextDrawer;
        }
        
        return null;
    }
    
    public async Task UpdateLeaderboard()
    {
        var leaderboard = _lobbyService.GetAllPlayersInLobby()
            .OrderByDescending(p => p.Points)
            .Select(p => new
            {
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
        var currentlyDrawingPlayer = _lobbyService.GetAllPlayersInLobby()[CurrentPlayerIdx];
        return player.Id == currentlyDrawingPlayer.Id;
    }

    // <summary>
    // Notify next user that is his turn and show them words for drawing.
    // </summar>
    private async Task NotifyNextPlayer()
    {
        var player = _lobbyService.GetAllPlayersInLobby()[CurrentPlayerIdx];
        Random random = new Random();
        
        await _hubContext.Clients.Groups("Lobby").SendAsync("Broadcast", new BroadcastResponse
          {
          Message = $"It's {player.Username} turn!",
          });

        await _hubContext.Clients.Client(player.Id).SendAsync("YourTurn", new
        {
            Words = _wordList.OrderBy(x => random.Next()).Take(3).ToArray()
        });
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
