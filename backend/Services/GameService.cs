using backend.Hubs;
using backend.Models;
using Microsoft.AspNetCore.SignalR;

namespace backend.Services;

public class GameService : IGameService
{
    private readonly ILobbyService _lobbyService;
    private readonly IHubContext<GameHub> _hubContext;
    private int _currentPlayerIdx { get; set; }
    private string _wordToDraw { get; set; }
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
        _currentPlayerIdx = 0;

        var player = _lobbyService.GetAllPlayersInLobby()[_currentPlayerIdx];

        Random random = new Random();
        await _hubContext.Clients.Client(player.Id).SendAsync("YourTurn", new
        {
          Words = _wordList.OrderBy(x => random.Next()).Take(3).ToArray()
        });
    }

    public async Task SelectWord(string word)
    {
      _wordToDraw = word;

      await _hubContext.Clients.All.SendAsync("WordSelected");
    }

    public async Task<bool> GuessWord(Player player, string word)
    {
        if (_lobbyService.GameIsStarted())
        {
            if (_wordToDraw.Equals(word, StringComparison.OrdinalIgnoreCase))
            {
                _lobbyService.AddPointsToPlayer(player.Id, 10);

                await UpdateLeaderboard();

                await NextTurn();
                
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Moves to the next player's turn and sends them new word options.
    /// </summary>
    public async Task NextTurn()
    {
      _currentPlayerIdx = (_currentPlayerIdx + 1) % _lobbyService.GetAllPlayersInLobby().Count;

      var player = _lobbyService.GetAllPlayersInLobby()[_currentPlayerIdx];

      await _hubContext.Clients.Groups("Lobby").SendAsync("NextTurn");

      Random random = new Random();
      await _hubContext.Clients.Client(player.Id).SendAsync("YourTurn", new
      {
          Words = _wordList.OrderBy(x => random.Next()).Take(3).ToArray()
      });

      _wordToDraw = string.Empty;
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
      var currentlyDrawingPlayer = _lobbyService.GetAllPlayersInLobby()[_currentPlayerIdx];
      return player.Id == currentlyDrawingPlayer.Id;
    }
}
