namespace Fidelity.Application.DTOs;

public class TransazioneDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int PuntoVenditaId { get; set; }
    public string PuntoVenditaNome { get; set; } = string.Empty;
    public int? ResponsabileId { get; set; }
    public string? ResponsabileNome { get; set; }
    public DateTime DataTransazione { get; set; }
    public decimal Importo { get; set; }
    public int PuntiGuadagnati { get; set; }
    public string? Note { get; set; }
}
