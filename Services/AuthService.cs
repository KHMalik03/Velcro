using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using velcro.Data;
using velcro.Models.DTOs;
using velcro.Models.Entities;
using velcro.Services.Interfaces;

namespace velcro.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(ApplicationDbContext db, IConfiguration config)
    {
        _db     = db;
        _config = config;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Vérifie l'unicité avant d'insérer pour avoir un message d'erreur clair
        if (await _db.Users.AnyAsync(u => u.Email    == request.Email))    throw new InvalidOperationException("Email déjà utilisé.");
        if (await _db.Users.AnyAsync(u => u.Username == request.Username)) throw new InvalidOperationException("Nom d'utilisateur déjà pris.");

        var user = new User
        {
            Id           = Guid.NewGuid(),
            Username     = request.Username,
            Email        = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // hashage bcrypt (coût par défaut = 11)
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email)
            ?? throw new UnauthorizedAccessException("Identifiants invalides.");

        // BCrypt.Verify recrée le hash à partir du mot de passe et le compare au hash stocké
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Identifiants invalides.");

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var stored = await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken)
            ?? throw new UnauthorizedAccessException("Refresh token invalide.");

        if (!stored.IsActive) // IsActive = non révoqué ET non expiré
            throw new UnauthorizedAccessException("Refresh token expiré ou révoqué.");

        stored.IsRevoked = true; // rotation : l'ancien token est invalidé
        await _db.SaveChangesAsync();

        return await BuildAuthResponseAsync(stored.User); // un nouveau couple est émis
    }

    public async Task RevokeTokenAsync(string token)
    {
        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token)
            ?? throw new UnauthorizedAccessException("Token introuvable.");

        stored.IsRevoked = true;
        await _db.SaveChangesAsync();
    }

    public async Task<UserDto> GetUserAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Utilisateur introuvable.");
        return ToUserDto(user);
    }

    // Génère l'access token + le refresh token et les retourne ensemble
    private async Task<AuthResponse> BuildAuthResponseAsync(User user)
    {
        var (accessToken, expiry) = GenerateAccessToken(user);
        var refreshToken          = await CreateRefreshTokenAsync(user.Id);
        return new AuthResponse(accessToken, refreshToken, expiry, ToUserDto(user));
    }

    // Crée un JWT signé en HS256 avec les claims de l'utilisateur (durée : 30 min par défaut)
    private (string token, DateTime expiry) GenerateAccessToken(User user)
    {
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpirationMinutes"]!));
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),    // identifiant unique
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("username", user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // identifiant unique du token
        };
        var token = new JwtSecurityToken(
            issuer:            _config["Jwt:Issuer"],
            audience:          _config["Jwt:Audience"],
            claims:            claims,
            expires:           expiry,
            signingCredentials: creds
        );
        return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
    }

    // Génère 64 octets aléatoires sécurisés → token opaque stocké en base (durée : 7 jours)
    private async Task<string> CreateRefreshTokenAsync(Guid userId)
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        var token = new RefreshToken
        {
            Id        = Guid.NewGuid(),
            Token     = Convert.ToBase64String(bytes),
            UserId    = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpirationDays"]!)),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.RefreshTokens.Add(token);
        await _db.SaveChangesAsync();
        return token.Token;
    }

    private static UserDto ToUserDto(User user) =>
        new(user.Id, user.Username, user.Email);
}
