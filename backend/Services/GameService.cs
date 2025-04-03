using backend.Hubs;
using backend.Models;
using Microsoft.AspNetCore.SignalR;

namespace backend.Services;

public class GameService : IGameService
{
    private readonly ILobbyService _lobbyService;
    private readonly IHubContext<GameHub> _hubContext;
    public string WordToGuess { get; set; } = "simpleword";
    
    public GameService(ILobbyService lobbyService, IHubContext<GameHub> hubContext)
    {
        _lobbyService = lobbyService;
        _hubContext = hubContext;
    }
    
    public async Task<bool> GuessWord(Player player, string word)
    {
        if (_lobbyService.GameIsStarted())
        {
            if (WordToGuess.Equals(word, StringComparison.OrdinalIgnoreCase))
            {
                _lobbyService.AddPointsToPlayer(player.Id, 10);
                
                return true;
            }
        }

        return false;
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
}