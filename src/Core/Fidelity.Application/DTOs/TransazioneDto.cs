namespace Fidelity.Application.DTOs;

/// <summary>
/// Transazione DTOs
/// </summary>

public record TransazioneDto
{
    public int Id { get; init; }
    public int ClienteId { get; init; }
    public string ClienteNome { get; init; } = string.Empty;
    public int? PuntoVenditaId { get; init; }
    public string? PuntoVenditaNome { get; init; }
    public int? ResponsabileId { get; init; }
    public string? ResponsabileNome { get; init; }
    public DateTime DataTransazione { get; init; }
    public decimal Importo { get; init; }
    public int PuntiGuadagnati { get; init; }
    public string Tipo { get; init; } = "Accumulo";
    public string? Note { get; init; }
}

public record TransazioneDetailDto : TransazioneDto
{
    public int ClienteId { get; init; }
    public string ClienteNome { get; init; } = string.Empty;
    public string? ResponsabileNome { get; init; }
}
