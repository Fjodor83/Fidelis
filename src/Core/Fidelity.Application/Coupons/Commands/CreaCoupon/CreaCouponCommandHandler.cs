using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.Common.Models;
using Fidelity.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Application.Coupons.Commands.CreaCoupon;

public class CreaCouponCommandHandler : IRequestHandler<CreaCouponCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public CreaCouponCommandHandler(IApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<Result<int>> Handle(CreaCouponCommand request, CancellationToken cancellationToken)
    {
        // Check unique code
        if (await _context.Coupons.AnyAsync(c => c.Codice == request.Codice, cancellationToken))
            return Result<int>.Failure("Esiste già un coupon con questo codice");
        
        var coupon = new Coupon
        {
            Codice = request.Codice.ToUpper(),
            Titolo = request.Titolo,
            Descrizione = request.Descrizione,
            ValoreSconto = request.ValoreSconto,
            TipoSconto = request.TipoSconto,
            DataInizio = request.DataInizio,
            DataScadenza = request.DataScadenza,
            Attivo = request.Attivo
        };
        
        // Validate business rules
        try
        {
            coupon.Validate();
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }
        
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Send email notifications to active customers
        if (coupon.Attivo)
        {
            try
            {
                var clientiAttivi = await _context.Clienti
                    .Where(c => c.Attivo && !string.IsNullOrEmpty(c.Email))
                    .ToListAsync(cancellationToken);

                foreach (var cliente in clientiAttivi)
                {
                    try
                    {
                        await _emailService.InviaEmailNuovoCouponAsync(
                            cliente.Email,
                            cliente.Nome,
                            coupon.Titolo,
                            coupon.Codice,
                            coupon.DataScadenza
                        );
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue sending to other customers
                        Console.WriteLine($"Errore invio email a {cliente.Email}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the coupon creation
                Console.WriteLine($"Errore durante l'invio delle email: {ex.Message}");
            }
        }
        
        return Result<int>.Success(coupon.Id);
    }
}
