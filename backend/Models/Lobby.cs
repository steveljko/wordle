namespace backend.Models;

public class Lobby
{
    public Guid Id { get; set; }
    public List<Player> Players { get; set; }
    
    public bool HasGameInProgress { get; set; } = false;
}