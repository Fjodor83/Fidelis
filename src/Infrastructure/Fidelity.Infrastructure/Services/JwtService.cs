using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fidelity.Application.Common.Interfaces;
using Fidelity.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Fidelity.Infrastructure.Services;

/// <summary>
/// JWT Service implementation - ISO 25000: Security
/// </summary>
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly IApplicationDbContext _context;

    public JwtService(IConfiguration configuration, IApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public async Task<(string Token, string RefreshToken)> GenerateTokensAsync(int userId, string role, CancellationToken cancellationToken = default)
    {
        var jwtId = Guid.NewGuid().ToString();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role)
        };

        // Add additional claims based on role
        if (role == "Cliente")
        {
            var cliente = await _context.Clienti.FindAsync(new object[] { userId }, cancellationToken);
            if (cliente != null)
            {
                claims.Add(new(ClaimTypes.Name, cliente.Email));
                claims.Add(new(ClaimTypes.Email, cliente.Email));
                claims.Add(new("CodiceFidelity", cliente.CodiceFidelity));
                claims.Add(new("NomeCompleto", cliente.NomeCompleto));
                claims.Add(new("Livello", cliente.Livello.ToString()));
            }
        }
        else
        {
            var responsabile = await _context.Responsabili.FindAsync(new object[] { userId }, cancellationToken);
            if (responsabile != null)
            {
                claims.Add(new(ClaimTypes.Name, responsabile.Username));
                claims.Add(new("NomeCompleto", responsabile.NomeCompleto ?? ""));
                if (responsabile.Email != null) claims.Add(new(ClaimTypes.Email, responsabile.Email));
            }
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Generate and store refresh token
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + "-" + Guid.NewGuid().ToString(),
            JwtId = jwtId,
            CreatedDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            IsUsed = false,
            IsRevoked = false,
            ClienteId = role == "Cliente" ? userId : null,
            ResponsabileId = role != "Cliente" ? userId : null
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return (accessToken, refreshToken.Token);
    }

    public (string Token, string JwtId) GenerateToken(Cliente cliente)
    {
        var jwtId = Guid.NewGuid().ToString();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(ClaimTypes.NameIdentifier, cliente.Id.ToString()),
            new(ClaimTypes.Name, cliente.Email),
            new(ClaimTypes.Email, cliente.Email),
            new(ClaimTypes.Role, "Cliente"),
            new("CodiceFidelity", cliente.CodiceFidelity),
            new("NomeCompleto", cliente.NomeCompleto),
            new("Livello", cliente.Livello.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), jwtId);
    }

    public (string Token, string JwtId) GenerateToken(Responsabile responsabile, int? puntoVenditaId = null)
    {
        var jwtId = Guid.NewGuid().ToString();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(ClaimTypes.NameIdentifier, responsabile.Id.ToString()),
            new(ClaimTypes.Name, responsabile.Username),
            new(ClaimTypes.Role, responsabile.Ruolo),
            new("NomeCompleto", responsabile.NomeCompleto ?? "")
        };

        if (responsabile.Email != null)
        {
            claims.Add(new Claim(ClaimTypes.Email, responsabile.Email));
        }

        if (puntoVenditaId.HasValue)
        {
            claims.Add(new Claim("PuntoVenditaId", puntoVenditaId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), jwtId);
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string? GetClaimValue(string token, string claimType)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }
        catch
        {
            return null;
        }
    }
}
