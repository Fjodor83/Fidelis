namespace Fidelity.Application.DTOs;

/// <summary>
/// Authentication DTOs
/// </summary>

public record LoginResponseDto
{
    public bool Success { get; init; }
    public string? Messaggio { get; init; }
    public string? Token { get; init; }
    public string? RefreshToken { get; init; }
    public UserInfoDto? User { get; init; }
    public ClienteDto? Cliente { get; init; }
}

public record UserInfoDto
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Ruolo { get; init; } = string.Empty;
    public string? CodiceFidelity { get; init; }
    public int? PuntoVenditaId { get; init; }
    public string? PuntoVenditaNome { get; init; }
    public int PuntiTotali { get; init; }
}

public record RefreshTokenRequestDto
{
    public string RefreshToken { get; init; } = string.Empty;
}
