using FlightAggregatorApi.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightAggregatorApi.API.Controllers;

/// <summary>
/// Controller for handling authentication requests.
/// </summary>
[Route("api/auth")]
[AllowAnonymous]
[ApiController]
public class AuthController(IJwtTokenService jwtTokenService) : ControllerBase
{
    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">The login request containing username and password.</param>
    /// <returns>An action result containing the JWT token if authentication is successful.</returns>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request is not { Username: "admin", Password: "password" }) //TODO: Replace with checking against database
            return Unauthorized("Invalid username or password.");

        var token = jwtTokenService.GenerateJwtToken(request.Username);
        return Ok(new { Token = token });
    }
}

/// <summary>
/// Represents a login request.
/// </summary>
public record LoginRequest
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public required string Password { get; init; }
}
