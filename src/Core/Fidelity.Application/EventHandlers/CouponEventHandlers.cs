using Fidelity.Application.Common.Interfaces;
using Fidelity.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fidelity.Application.EventHandlers;

/// <summary>
/// Handles CouponCreatoEvent
/// </summary>
public class CouponCreatoEventHandler : INotificationHandler<CouponCreatoEvent>
{
    private readonly ILogger<CouponCreatoEventHandler> _logger;

    public CouponCreatoEventHandler(ILogger<CouponCreatoEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CouponCreatoEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[EVENT] Coupon creato: {CouponId}, Codice: {Codice}, AutoAssegnazione: {Auto}",
            notification.CouponId,
            notification.Codice,
            notification.AssegnazioneAutomatica);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles CouponUtilizzatoEvent - tracks usage statistics
/// </summary>
public class CouponUtilizzatoEventHandler : INotificationHandler<CouponUtilizzatoEvent>
{
    private readonly ILogger<CouponUtilizzatoEventHandler> _logger;

    public CouponUtilizzatoEventHandler(ILogger<CouponUtilizzatoEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CouponUtilizzatoEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[EVENT] Coupon utilizzato: {CouponId}, Codice: {Codice}, Utilizzi totali: {Utilizzi}",
            notification.CouponId,
            notification.Codice,
            notification.UtilizziTotali);

        // Here you could:
        // - Update analytics dashboard
        // - Check if global limit reached and disable coupon
        // - Track conversion rates

        return Task.CompletedTask;
    }
}
