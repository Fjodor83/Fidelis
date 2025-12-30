namespace Fidelity.Client.Services.Interfaces;

public class TransazioneRequest
{
    public int ClienteId { get; set; }
    public int? PuntoVenditaId { get; set; }
    public decimal Importo { get; set; }
    public string? Note { get; set; }
    public string Tipo { get; set; } = "Accumulo";
}