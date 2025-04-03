using backend.Models;

namespace backend.Services;

public interface IGameService
{
    Task<bool> GuessWord(Player player, string word);
    Task UpdateLeaderboard();
}