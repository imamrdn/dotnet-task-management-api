using TaskManagement.Api.DTOs;

namespace TaskManagement.Api.Services;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request);
    Task LogoutAsync(LogoutRequest request);
}
