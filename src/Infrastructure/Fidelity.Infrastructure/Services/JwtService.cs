using Fidelity.Application.Common.Interfaces;
using Fidelity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Fidelity.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public JwtService(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(
        int userId, 
        string role, 
        CancellationToken cancellationToken)
    {
        // Generate JWT Access Token
        var jwtId = Guid.NewGuid().ToString();
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, jwtId),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1), // 1 hour
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Generate Refresh Token
        var refreshToken = GenerateRefreshToken();

        // Save refresh token to database
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            JwtId = jwtId,
            CreatedDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            IsRevoked = false,
            ClienteId = role == "Cliente" ? userId : null,
            ResponsabileId = role != "Cliente" ? userId : null
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return (accessToken, refreshToken);
    }

    public async Task<(bool IsValid, int? UserId, string? Role)> ValidateRefreshTokenAsync(
        string refreshToken, 
        CancellationToken cancellationToken)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (storedToken == null || !storedToken.IsValid())
            return (false, null, null);

        // Determine user ID and role
        int? userId = storedToken.ClienteId ?? storedToken.ResponsabileId;
        string? role = storedToken.ClienteId.HasValue ? "Cliente" : "Admin";

        return (true, userId, role);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
