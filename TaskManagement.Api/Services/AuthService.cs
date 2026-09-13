using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Services;

public class AuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext dbContext, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Name is required");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required");
        }

        var emailExists = await _dbContext.Users.AnyAsync(u => u.Email == request.Email);

        if (emailExists)
        {
            throw new ArgumentException("Email is already registered");
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Role = "User"
        };

        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("User {UserId} registered", user.Id);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required");
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            _logger.LogWarning("Login failed: invalid credentials");
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var passwordHasher = new PasswordHasher<User>();
        PasswordVerificationResult result;

        try
        {
            result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        }
        catch (FormatException)
        {
            _logger.LogWarning("Login failed: invalid credentials");
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (result == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Login failed: invalid credentials");
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = HashRefreshToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id
        });
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("User {UserId} logged in", user.Id);
        return new AuthResponse(token, refreshToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new ArgumentException("Refresh token is required");
        }

        var tokenHash = HashRefreshToken(request.RefreshToken);
        var refreshToken = await _dbContext.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            _logger.LogWarning("Refresh token rejected");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var newRefreshToken = GenerateRefreshToken();
        var newRefreshTokenHash = HashRefreshToken(newRefreshToken);

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReplacedByTokenHash = newRefreshTokenHash;

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = newRefreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            UserId = refreshToken.UserId
        });

        await _dbContext.SaveChangesAsync();

        var accessToken = GenerateJwtToken(refreshToken.User);
        _logger.LogInformation("Refresh token rotated for user {UserId}", refreshToken.UserId);
        return new AuthResponse(accessToken, newRefreshToken);
    }

    public async Task LogoutAsync(LogoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new ArgumentException("Refresh token is required");
        }

        var tokenHash = HashRefreshToken(request.RefreshToken);
        var refreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash);

        if (refreshToken is null)
        {
            return;
        }

        if (refreshToken.RevokedAt is null)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Refresh token revoked for user {UserId}", refreshToken.UserId);
        }
    }

    private string GenerateJwtToken(User user)
    {
        var key = _configuration["Jwt:Key"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("JWT key is not configured");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    private static string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes);
    }
}
