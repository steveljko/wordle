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

[Route("/lobby")]
public class LobbyController : Controller
{
    private readonly LobbyService _lobbyService;
    private readonly IHubContext<GameHub> _hubContext;
    private readonly IHttpContextAccessor _httpContext;
    
    public LobbyController(LobbyService lobbyService, IHubContext<GameHub> hubContext, IHttpContextAccessor httpContext)
    {
        _lobbyService = lobbyService;
        _hubContext = hubContext;
        _httpContext = httpContext;
    }
    
    [HttpPost]
    public async Task<IActionResult> Join([FromBody] JoinRequest request)
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
}