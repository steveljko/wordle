using backend.Models;

namespace backend.Services;

public class LobbyService : ILobbyService
{
    public Lobby lobby { get; private set; } = new Lobby
    {
        Id = Guid.NewGuid(),
        Players = new List<Player>(),
        HasGameInProgress = false
    };
    
    public void AddPlayer(Player player)
    {
        lobby.Players.Add(player);
    }
    
    public void RemovePlayer(string Id)
    {
        var player = lobby.Players.FirstOrDefault(p => p.Id == Id);
        if (player != null)
        {
            lobby.Players.Remove(player);
        }
    }

    public Player FindPlayerById(string Id)
    {
        return lobby.Players.FirstOrDefault(p => p.Id == Id);
    }

    public List<Player> GetAllPlayersInLobby()
    {
        return lobby.Players;
    }

    public void StartGame()
    {
        lobby.HasGameInProgress = true;
    }

    public bool GameIsStarted()
    {
        return lobby.HasGameInProgress == true;
    }
    
    public void AddPointsToPlayer(string playerId, int points)
    {
        var player = FindPlayerById(playerId);
        if (player != null)
        {
            player.Points += points;
        }
    }
}