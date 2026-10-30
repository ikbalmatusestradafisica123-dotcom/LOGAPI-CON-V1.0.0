using LOGAPI_CON.Services;
using LOGAPI_CON.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;


namespace LOGAPI_CON.Controllers;

[ApiController]
[Route("api/[controller]")]

public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);

        if (result is null)
        {
            return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
        }

        return Ok(result);
    }

}