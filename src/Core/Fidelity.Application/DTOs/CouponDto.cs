namespace Fidelity.Application.DTOs;

/// <summary>
/// Coupon DTOs
/// </summary>

public record CouponDto
{
    public int Id { get; init; }
    public string Codice { get; init; } = string.Empty;
    public string Titolo { get; init; } = string.Empty;
    public string? Descrizione { get; init; }
    public decimal ValoreSconto { get; init; }
    public string TipoSconto { get; init; } = "Percentuale";
    public DateTime DataInizio { get; init; }
    public DateTime DataScadenza { get; init; }
    public bool Attivo { get; init; }
    public int? LimiteUtilizzoGlobale { get; init; }
    public int UtilizziTotali { get; init; }
    public decimal? ImportoMinimoOrdine { get; init; }
}

public record CouponAssegnatoDto
{
    public int Id { get; init; }
    public int CouponId { get; init; }
    public string Codice { get; init; } = string.Empty;
    public string Titolo { get; init; } = string.Empty;
    public string? Descrizione { get; init; }
    public decimal ValoreSconto { get; init; }
    public string TipoSconto { get; init; } = "Percentuale";
    public DateTime DataInizio { get; init; }
    public DateTime DataScadenza { get; init; }
    public DateTime DataAssegnazione { get; init; }
    public bool Utilizzato { get; init; }
    public DateTime? DataUtilizzo { get; init; }
    public decimal? ImportoMinimoOrdine { get; init; }

    public bool IsScaduto => !Utilizzato && DataScadenza < DateTime.UtcNow;
    public bool IsAttivo => !Utilizzato && !IsScaduto;
}
