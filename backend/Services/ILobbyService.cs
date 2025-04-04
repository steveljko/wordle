using backend.Models;

namespace backend.Services;

public interface ILobbyService
{
    void AddPlayer(Player player);
    void RemovePlayer(string id);
    Player FindPlayerById(string id);
    List<Player> GetAllPlayersInLobby();
    void StartGame();
    bool GameIsStarted();
    void AddPointsToPlayer(string playerId, int points);
}