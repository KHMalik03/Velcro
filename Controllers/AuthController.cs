using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using velcro.Models.DTOs;
using velcro.Services.Interfaces;

namespace velcro.Controllers;

// [ApiController] : active la validation automatique du ModelState + binding JSON
// [Route] : préfixe commun à tous les endpoints de ce controller
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    // L'injection de dépendance fournit l'implémentation concrète (AuthService)
    public AuthController(IAuthService auth) => _auth = auth;

    // Crée un compte → retourne un access token + refresh token
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            return Ok(await _auth.RegisterAsync(request));
        }
        catch (InvalidOperationException ex) // email ou username déjà pris
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Vérifie les identifiants → retourne un access token (30 min) + refresh token (7 jours)
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            return Ok(await _auth.LoginAsync(request));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Identifiants invalides." });
        }
    }

    // Échange un refresh token valide contre un nouveau couple access/refresh token (rotation)
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            return Ok(await _auth.RefreshTokenAsync(request));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    // Révoque le refresh token → déconnexion complète
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        try
        {
            await _auth.RevokeTokenAsync(request.RefreshToken);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    // Retourne le profil de l'utilisateur connecté (lu depuis les claims du JWT)
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        // ClaimTypes.NameIdentifier correspond au champ "sub" du JWT
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(sub, out var userId))
            return Unauthorized();

        try
        {
            return Ok(await _auth.GetUserAsync(userId));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
