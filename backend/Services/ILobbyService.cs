using backend.Models;

namespace backend.Services;

public interface ILobbyService
{
    void AddPlayer(Player player);
    void RemovePlayer(string id);
    Player FindPlayerById(string id);
    List<Player> GetAllPlayersInLobby();
    Player GetPlayerWithMostPoints();
    void StartGame();
    void StopGame();
    bool GameIsStarted();
    void AddPointsToPlayer(string playerId, int points);
}
