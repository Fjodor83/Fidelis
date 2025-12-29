namespace Fidelity.Application.DTOs;

/// <summary>
/// Cliente DTOs - ISO 25000: Maintainability (separation of concerns)
/// </summary>

public record ClienteDto
{
    public int Id { get; init; }
    public string CodiceFidelity { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string Cognome { get; init; } = string.Empty;
    public string NomeCompleto => $"{Nome} {Cognome}";
    public string Email { get; init; } = string.Empty;
    public string? Telefono { get; init; }
    public int PuntiTotali { get; init; }
    public int PuntiDisponibili { get; init; }
    public string Livello { get; init; } = "Bronze";
    public bool Attivo { get; init; }
    public DateTime DataRegistrazione { get; init; }
}

public record ClienteDetailDto : ClienteDto
{
    public DateTime DataRegistrazione { get; init; }
    public string? PuntoVenditaRegistrazione { get; init; }
    public List<TransazioneDto> UltimeTransazioni { get; init; } = new();
    public List<CouponAssegnatoDto> CouponAttivi { get; init; } = new();
}

public record ClienteSearchResult
{
    public int Id { get; init; }
    public string CodiceFidelity { get; init; } = string.Empty;
    public string NomeCompleto { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int PuntiTotali { get; init; }
}
