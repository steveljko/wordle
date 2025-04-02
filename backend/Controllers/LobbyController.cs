using System.Net;
using backend.Hubs;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using CookieOptions = Microsoft.AspNetCore.Http.CookieOptions;

namespace backend.Controllers;

public class JoinRequest
{
    public string Username { get; set; }
}

public class LobbyController : Controller
{
    private readonly LobbyService _lobbyService;
    private readonly IHubContext<GameHub> _hubContext;
    
    public LobbyController(LobbyService lobbyService, IHubContext<GameHub> hubContext)
    {
        _lobbyService = lobbyService;
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
    public IActionResult Leave()
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
        
        return Ok(new { Message = "You have successfully logged out." });
    }

    [HttpPost("/startGame")]
    public async Task<IActionResult> StartGame()
    {
        if (_lobbyService.GetAllPlayersInLobby().Count <= 2)
        {
            return BadRequest(new
            {
                Code = "NotEnoughPlayersInLobby",
                Message = "There are not enough players in the lobby to start the game."
            });
        }

        await _hubContext.Clients.Groups("Lobby").SendAsync("GameStarted");

        _lobbyService.StartGame();
        
        return Ok();
    }
}