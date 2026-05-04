namespace velcro.Models.DTOs;

public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);
public record UserDto(Guid Id, string Username, string Email);
public record AuthResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiry, UserDto User);
