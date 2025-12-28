using Fidelity.Domain.Common;

namespace Fidelity.Domain.Entities;

public class Coupon : BaseEntity
{
    public required string Codice { get; set; }
    public required string Titolo { get; set; }
    public string? Descrizione { get; set; }
    public decimal ValoreSconto { get; set; }
    public required string TipoSconto { get; set; } // "Percentuale" o "Fisso"
    public DateTime DataInizio { get; set; }
    public DateTime DataScadenza { get; set; }
    public bool Attivo { get; set; } = true;
    
    // Business rules
    public bool IsValido()
    {
        var now = DateTime.UtcNow;
        return Attivo && now >= DataInizio && now <= DataScadenza;
    }
    
    public void Validate()
    {
        if (ValoreSconto <= 0)
            throw new ArgumentException("Il valore sconto deve essere maggiore di zero");
        
        if (TipoSconto == "Percentuale" && ValoreSconto > 100)
            throw new ArgumentException("Lo sconto percentuale non può superare 100%");
        
        if (DataScadenza <= DataInizio)
            throw new ArgumentException("La data scadenza deve essere successiva alla data inizio");
        
        if (string.IsNullOrWhiteSpace(Codice))
            throw new ArgumentException("Il codice coupon è obbligatorio");
    }
    
    public decimal CalcolaSconto(decimal importo)
    {
        if (TipoSconto == "Percentuale")
            return importo * (ValoreSconto / 100);
        
        return ValoreSconto; // Fisso
    }
}

public class CouponAssegnato : BaseEntity
{
    public int CouponId { get; set; }
    public int ClienteId { get; set; }
    public DateTime DataAssegnazione { get; set; }
    public DateTime? DataUtilizzo { get; set; }
    public bool Utilizzato { get; set; } = false;
    
    public bool PuoEssereUtilizzato()
    {
        return !Utilizzato && DataUtilizzo == null;
    }
    
    public void SegnaUtilizzato()
    {
        if (Utilizzato)
            throw new InvalidOperationException("Coupon già utilizzato");
        
        Utilizzato = true;
        DataUtilizzo = DateTime.UtcNow;
    }
}
