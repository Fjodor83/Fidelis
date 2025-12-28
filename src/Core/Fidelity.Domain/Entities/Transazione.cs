using Fidelity.Domain.Common;

namespace Fidelity.Domain.Entities;

public class Transazione : BaseEntity
{
    public required int ClienteId { get; set; }
    public required int PuntoVenditaId { get; set; }
    public int? ResponsabileId { get; set; }
    
    public DateTime DataTransazione { get; set; }
    public decimal Importo { get; set; }
    public int PuntiGuadagnati { get; set; }
    public string? Note { get; set; }
    
    // Business logic
    public static int CalcolaPunti(decimal importo)
    {
        // 1 punto ogni 10 euro
        return (int)(importo / 10);
    }
    
    public void Validate()
    {
        if (Importo <= 0)
            throw new ArgumentException("L'importo deve essere maggiore di zero");
        
        if (PuntiGuadagnati < 0)
            throw new ArgumentException("I punti guadagnati non possono essere negativi");
        
        if (DataTransazione > DateTime.UtcNow)
            throw new ArgumentException("La data transazione non può essere nel futuro");
    }
}
