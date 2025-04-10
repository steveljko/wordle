namespace backend.Responses;

public class BroadcastResponse
{
  public bool Success { get; set; } = true;
  public string Message { get; set; }
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
