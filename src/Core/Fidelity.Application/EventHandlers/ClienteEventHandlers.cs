using Fidelity.Application.Common.Interfaces;
using Fidelity.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fidelity.Application.EventHandlers;

/// <summary>
/// Handles ClienteRegistratoEvent - sends welcome notifications
/// ISO 25000: Maintainability (loose coupling via events)
/// </summary>
public class ClienteRegistratoEventHandler : INotificationHandler<ClienteRegistratoEvent>
{
    private readonly ILogger<ClienteRegistratoEventHandler> _logger;

    public ClienteRegistratoEventHandler(ILogger<ClienteRegistratoEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ClienteRegistratoEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[EVENT] Cliente registrato: {ClienteId}, Codice: {CodiceFidelity}, Email: {Email}",
            notification.ClienteId,
            notification.CodiceFidelity,
            notification.Email);

        // Additional actions can be triggered here:
        // - Analytics tracking
        // - CRM integration
        // - Marketing automation

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles PuntiAggiuntiEvent - logs and potentially triggers notifications
/// </summary>
public class PuntiAggiuntiEventHandler : INotificationHandler<PuntiAggiuntiEvent>
{
    private readonly ILogger<PuntiAggiuntiEventHandler> _logger;

    public PuntiAggiuntiEventHandler(ILogger<PuntiAggiuntiEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(PuntiAggiuntiEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[EVENT] Punti aggiunti: Cliente {ClienteId}, +{Punti} punti, Totale: {Totale}, Transazione: {TransazioneId}",
            notification.ClienteId,
            notification.PuntiAggiunti,
            notification.PuntiTotali,
            notification.TransazioneId);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles LivelloFedeltaCambiatoEvent - notifies customer of level upgrade
/// </summary>
public class LivelloFedeltaCambiatoEventHandler : INotificationHandler<LivelloFedeltaCambiatoEvent>
{
    private readonly ILogger<LivelloFedeltaCambiatoEventHandler> _logger;
    private readonly IEmailService _emailService;
    private readonly IApplicationDbContext _context;

    public LivelloFedeltaCambiatoEventHandler(
        ILogger<LivelloFedeltaCambiatoEventHandler> logger,
        IEmailService emailService,
        IApplicationDbContext context)
    {
        _logger = logger;
        _emailService = emailService;
        _context = context;
    }

    public async Task Handle(LivelloFedeltaCambiatoEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[EVENT] Livello cambiato: Cliente {ClienteId}, {VecchioLivello} -> {NuovoLivello}",
            notification.ClienteId,
            notification.VecchioLivello,
            notification.NuovoLivello);

        // Only send email for upgrades
        if (notification.NuovoLivello > notification.VecchioLivello)
        {
            try
            {
                var cliente = await _context.Clienti.FindAsync(
                    new object[] { notification.ClienteId },
                    cancellationToken);

                if (cliente != null)
                {
                    await _emailService.InviaEmailLivelloRaggiuntoAsync(
                        cliente.Email,
                        cliente.Nome,
                        notification.NuovoLivello.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore invio email livello raggiunto per cliente {ClienteId}",
                    notification.ClienteId);
            }
        }
    }
}
