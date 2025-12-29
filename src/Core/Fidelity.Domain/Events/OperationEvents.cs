using Fidelity.Domain.Common;

namespace Fidelity.Domain.Events;

public class TransazioneRegistrataEvent : DomainEventBase
{
    public int TransazioneId { get; }
    public int ClienteId { get; }
    public decimal Importo { get; }
    public int PuntiGuadagnati { get; }

    public TransazioneRegistrataEvent(int transazioneId, int clienteId, decimal importo, int puntiGuadagnati)
    {
        TransazioneId = transazioneId;
        ClienteId = clienteId;
        Importo = importo;
        PuntiGuadagnati = puntiGuadagnati;
    }
}

public class CouponCreatoEvent : DomainEventBase
{
    public int CouponId { get; }
    public string Titolo { get; }
    public string Codice { get; }
    public bool AssegnazioneAutomatica { get; }

    public CouponCreatoEvent(int couponId, string titolo, string codice, bool assegnazioneAutomatica)
    {
        CouponId = couponId;
        Titolo = titolo;
        Codice = codice;
        AssegnazioneAutomatica = assegnazioneAutomatica;
    }
}

public class CouponUtilizzatoEvent : DomainEventBase
{
    public int CouponId { get; }
    public int ClienteId { get; }
    public string Codice { get; }
    public int UtilizziTotali { get; }

    public CouponUtilizzatoEvent(int couponId, int clienteId, string codice, int utilizziTotali)
    {
        CouponId = couponId;
        ClienteId = clienteId;
        Codice = codice;
        UtilizziTotali = utilizziTotali;
    }
}
