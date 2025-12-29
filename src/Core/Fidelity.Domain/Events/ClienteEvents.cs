using Fidelity.Domain.Common;

namespace Fidelity.Domain.Events;

public class ClienteRegistratoEvent : DomainEventBase
{
    public int ClienteId { get; }
    public string CodiceFidelity { get; }
    public string Email { get; }

    public ClienteRegistratoEvent(int clienteId, string codiceFidelity, string email)
    {
        ClienteId = clienteId;
        CodiceFidelity = codiceFidelity;
        Email = email;
    }
}

public class PuntiAggiuntiEvent : DomainEventBase
{
    public int ClienteId { get; }
    public int PuntiAggiunti { get; }
    public int PuntiTotali { get; }
    public int TransazioneId { get; }

    public PuntiAggiuntiEvent(int clienteId, int puntiAggiunti, int puntiTotali, int transazioneId)
    {
        ClienteId = clienteId;
        PuntiAggiunti = puntiAggiunti;
        PuntiTotali = puntiTotali;
        TransazioneId = transazioneId;
    }
}

public class PuntiSpesiEvent : DomainEventBase
{
    public int ClienteId { get; }
    public int PuntiSpesi { get; }
    public int NuovoSaldo { get; }

    public PuntiSpesiEvent(int clienteId, int puntiSpesi, int nuovoSaldo)
    {
        ClienteId = clienteId;
        PuntiSpesi = puntiSpesi;
        NuovoSaldo = nuovoSaldo;
    }
}

public class LivelloFedeltaCambiatoEvent : DomainEventBase
{
    public int ClienteId { get; }
    public Entities.LivelloFedelta VecchioLivello { get; }
    public Entities.LivelloFedelta NuovoLivello { get; }

    public LivelloFedeltaCambiatoEvent(int clienteId, Entities.LivelloFedelta vecchioLivello, Entities.LivelloFedelta nuovoLivello)
    {
        ClienteId = clienteId;
        VecchioLivello = vecchioLivello;
        NuovoLivello = nuovoLivello;
    }
}
