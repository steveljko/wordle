using backend.Hubs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using CookieOptions = Microsoft.AspNetCore.Http.CookieOptions;

namespace backend.Controllers;

public class JoinRequest
{
    public string Username { get; set; }
}

public class SelectWordRequest
{
    public string Word { get; set; }
}

public class LobbyController : Controller
{
    private readonly ILobbyService _lobbyService;
    private readonly IGameService _gameService;
    private readonly IWordService _wordService;
    private readonly IHubContext<GameHub> _hubContext;
    
    public LobbyController(
        ILobbyService lobbyService,
        IGameService gameService,
        IWordService wordService,
        IHubContext<GameHub> hubContext)
    {
        _lobbyService = lobbyService;
        _gameService = gameService;
        _wordService = wordService;
        _hubContext = hubContext;
    }
    
    [HttpPost("/lobby")]
    public IActionResult Join([FromBody] JoinRequest request)
    {
        if (string.IsNullOrEmpty(request.Username))
        {
            return BadRequest(new
            {
                Code = "EmptyUsername",
                Message = "You need to provide valid username to join lobby."
            });
        }

        Response.Cookies.Append("username", request.Username, new CookieOptions
        {
            HttpOnly = false,
            Expires = DateTimeOffset.UtcNow.AddDays(1)
        });

        return Ok();
    }
    
    [HttpDelete("/leave")]
    public async Task<IActionResult> Leave()
    {
        if (!Request.Cookies.ContainsKey("username"))
        {
            return BadRequest(new
            {
                Code = "EmptyUsername",
                Message = "You are not logged in!"
            });
        }
        
        Response.Cookies.Delete("username");

        await _hubContext.Clients.Groups("Lobby").SendAsync("UserLeft", new
            {
            Players = _lobbyService.GetAllPlayersInLobby()
            });
        
        return Ok(new { Message = "You have successfully logged out." });
    }

    [HttpPost("/startGame")]
    public async Task<IActionResult> StartGame()
    {
        if (_lobbyService.GetAllPlayersInLobby().Count < 2)
        {
            return BadRequest(new
            {
                Code = "NotEnoughPlayersInLobby",
                Message = "There are not enough players in the lobby to start the game."
            });
        }

        _lobbyService.StartGame();
        await _hubContext.Clients.Groups("Lobby").SendAsync("GameStarted");
        await _gameService.OnStart();
        
        return Ok();
    }

    [HttpDelete("/stopGame")]
    public async Task<IActionResult> StopGame()
    {
        _lobbyService.StopGame();
        await _hubContext.Clients.Groups("Lobby").SendAsync("GameStopped");
        
        return Ok();
    }

    [HttpPost("/select")]
    public async Task<IActionResult> SelectWord([FromBody] SelectWordRequest request)
    {
      if (string.IsNullOrEmpty(request.Word))
      {
        return BadRequest(new {
          Message = "You must choose word."
        });
      }

      // check if the word provided by the user is valid word to choose.
      if (!_wordService.IsWordAvailable(request.Word))
      {
        return BadRequest(new {
            Message = "The word you provided is not available for selection."
        });
      }

      await _gameService.SelectWord(request.Word);

      return Ok(new { Success = true });
    }
}
