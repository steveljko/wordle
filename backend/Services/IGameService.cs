using backend.Models;

namespace backend.Services;

public interface IGameService
{
    Task OnStart();
    Task SelectWord(string word);
    Task<bool> GuessWord(Player player, string word);
    Task<Player> NextTurn();
    Task UpdateLeaderboard();
    bool IsCurrentDrawer(Player player);
}
