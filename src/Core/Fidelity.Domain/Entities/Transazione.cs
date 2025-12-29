using Fidelity.Domain.Common;

namespace Fidelity.Domain.Entities;

public class Transazione : BaseEntity
{
    public required int ClienteId { get; set; }
    public required int PuntoVenditaId { get; set; }
    public int? ResponsabileId { get; set; }

    public DateTime DataTransazione { get; set; }
    public decimal ImportoSpesa { get; set; }
    public int PuntiAssegnati { get; set; }
    public string? Note { get; set; }
    public int? CouponAssegnatoId { get; set; }
    public TipoTransazione Tipo { get; set; } = TipoTransazione.Accumulo;

    // Navigation properties
    public virtual Cliente Cliente { get; set; } = null!;
    public virtual PuntoVendita PuntoVendita { get; set; } = null!;
    public virtual Responsabile? Responsabile { get; set; }
    public virtual CouponAssegnato? CouponAssegnato { get; set; }

    // Business logic
    public static int CalcolaPunti(decimal importo)
    {
        // 1 punto ogni 10 euro
        return (int)(importo / 10);
    }

    public static Transazione CreaAccumulo(
        int clienteId,
        int puntoVenditaId,
        decimal importoSpesa,
        int puntiAssegnati,
        int? responsabileId = null,
        string? note = null)
    {
        return new Transazione
        {
            ClienteId = clienteId,
            PuntoVenditaId = puntoVenditaId,
            ResponsabileId = responsabileId,
            DataTransazione = DateTime.UtcNow,
            ImportoSpesa = importoSpesa,
            PuntiAssegnati = puntiAssegnati,
            Note = note,
            Tipo = TipoTransazione.Accumulo
        };
    }

    public void Validate()
    {
        if (ImportoSpesa <= 0)
            throw new ArgumentException("L'importo deve essere maggiore di zero");

        if (PuntiAssegnati < 0)
            throw new ArgumentException("I punti guadagnati non possono essere negativi");

        if (DataTransazione > DateTime.UtcNow)
            throw new ArgumentException("La data transazione non può essere nel futuro");
    }
}

public enum TipoTransazione
{
    Accumulo,
    Riscatto,
    Storno,
    Regalo
}


