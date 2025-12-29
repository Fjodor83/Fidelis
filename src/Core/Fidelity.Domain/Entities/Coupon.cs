using Fidelity.Domain.Common;
using Fidelity.Domain.Events;

namespace Fidelity.Domain.Entities;

public enum TipoSconto
{
    Percentuale,
    Fisso
}

public class Coupon : SoftDeleteEntity
{
    public required string Codice { get; set; }
    public required string Titolo { get; set; }
    public string? Descrizione { get; set; }
    public decimal ValoreSconto { get; set; }
    public TipoSconto TipoSconto { get; set; }
    public DateTime DataInizio { get; set; }
    public DateTime DataScadenza { get; set; }
    public bool Attivo { get; set; } = true;
    public bool AssegnazioneAutomatica { get; set; } = false;
    public int? LimiteUtilizzoGlobale { get; set; }
    public int? LimiteUtilizzoPerCliente { get; set; }
    public decimal? ImportoMinimoOrdine { get; set; }
    public int? PuntiRichiesti { get; set; }
    public bool IsCouponBenvenuto { get; set; } = false;
    public LivelloFedelta? LivelloMinimoRichiesto { get; set; }
    public int UtilizziTotali { get; private set; } = 0;

    // Relationships
    public virtual ICollection<CouponAssegnato> CouponAssegnati { get; set; } = new List<CouponAssegnato>();

    // Business rules
    public bool IsValido()
    {
        var now = DateTime.UtcNow;
        return Attivo && !IsDeleted && now >= DataInizio && now <= DataScadenza;
    }

    public void Validate()
    {
        if (ValoreSconto <= 0)
            throw new ArgumentException("Il valore sconto deve essere maggiore di zero");

        if (TipoSconto == TipoSconto.Percentuale && ValoreSconto > 100)
            throw new ArgumentException("Lo sconto percentuale non può superare 100%");

        if (DataScadenza <= DataInizio)
            throw new ArgumentException("La data scadenza deve essere successiva alla data inizio");

        if (string.IsNullOrWhiteSpace(Codice))
            throw new ArgumentException("Il codice coupon è obbligatorio");
    }

    public bool PuoEssereAssegnatoA(Cliente cliente)
    {
        if (!Attivo || IsDeleted) return false;
        if (DataScadenza < DateTime.UtcNow) return false;
        
        // Verifica livello minimo
        if (LivelloMinimoRichiesto.HasValue && cliente.Livello < LivelloMinimoRichiesto.Value) return false;
        
        // Verifica se già assegnato (se c'è limite per cliente)
        if (LimiteUtilizzoPerCliente.HasValue)
        {
            var assegnazioniCount = CouponAssegnati.Count(ca => ca.ClienteId == cliente.Id);
            if (assegnazioniCount >= LimiteUtilizzoPerCliente.Value) return false;
        }

        return true;
    }

    public decimal CalcolaSconto(decimal importo)
    {
        if (TipoSconto == TipoSconto.Percentuale)
            return importo * (ValoreSconto / 100);

        return ValoreSconto; // Fisso
    }

    public void IncrementaUtilizzi()
    {
        UtilizziTotali++;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum MotivoAssegnazione
{
    Automatico,
    Manuale,
    Premio
}

public class CouponAssegnato : BaseEntity
{
    public int CouponId { get; set; }
    public virtual Coupon Coupon { get; set; } = null!;
    
    public int ClienteId { get; set; }
    public virtual Cliente Cliente { get; set; } = null!;
    
    public DateTime DataAssegnazione { get; set; }
    public DateTime? DataUtilizzo { get; set; }
    public bool Utilizzato { get; set; } = false;
    
    public string? AssegnatoDa { get; set; }
    public MotivoAssegnazione Motivo { get; set; } = MotivoAssegnazione.Manuale;
    
    public int? ResponsabileUtilizzoId { get; set; }
    public virtual Responsabile? ResponsabileUtilizzo { get; set; }
    
    public int? PuntoVenditaUtilizzoId { get; set; }
    public virtual PuntoVendita? PuntoVenditaUtilizzo { get; set; }
    public int? TransazioneUtilizzoId { get; set; }
    public virtual Transazione? TransazioneUtilizzo { get; set; }

    public static CouponAssegnato Crea(int couponId, int clienteId, MotivoAssegnazione motivo, string? assegnatoDa = null)
    {
        return new CouponAssegnato
        {
            CouponId = couponId,
            ClienteId = clienteId,
            Motivo = motivo,
            AssegnatoDa = assegnatoDa,
            DataAssegnazione = DateTime.UtcNow,
            Utilizzato = false
        };
    }

    public bool PuoEssereUtilizzato()
    {
        return !Utilizzato && DataUtilizzo == null;
    }

    public void Utilizza(int? responsabileId = null, int? puntoVenditaId = null)
    {
        if (Utilizzato)
            throw new InvalidOperationException("Coupon già utilizzato");

        Utilizzato = true;
        DataUtilizzo = DateTime.UtcNow;
        PuntoVenditaUtilizzoId = puntoVenditaId;
        ResponsabileUtilizzoId = responsabileId;

        AddDomainEvent(new CouponUtilizzatoEvent(CouponId, ClienteId, Coupon.Codice, Coupon.UtilizziTotali));
    }
}


