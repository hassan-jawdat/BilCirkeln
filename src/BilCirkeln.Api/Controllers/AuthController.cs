using BilCirkeln.Api.DTOs;
using BilCirkeln.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BilCirkeln.Api.Controllers;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    // Dependency injection: ASP.NET Core creates the AuthService and passes it in.
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto) =>
        ToActionResult(await _authService.RegisterAsync(dto));

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto) =>
        ToActionResult(await _authService.LoginAsync(dto));
}
