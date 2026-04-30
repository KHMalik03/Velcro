using velcro.Models.DTOs;

namespace velcro.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task RevokeTokenAsync(string token);
    Task<UserDto> GetUserAsync(Guid userId);
}
