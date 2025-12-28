using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.Common.Models;
using Fidelity.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Application.Coupons.Commands.AssegnaCoupon;

public class AssegnaCouponCommandHandler : IRequestHandler<AssegnaCouponCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public AssegnaCouponCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(AssegnaCouponCommand request, CancellationToken cancellationToken)
    {
        // Verify coupon exists and is valid
        var coupon = await _context.Coupons.FindAsync(new object[] { request.CouponId }, cancellationToken);
        if (coupon == null)
            return Result<int>.Failure("Coupon non trovato");
        
        if (!coupon.IsValido())
            return Result<int>.Failure("Il coupon non è valido o è scaduto");
        
        // Verify cliente exists
        var cliente = await _context.Clienti.FindAsync(new object[] { request.ClienteId }, cancellationToken);
        if (cliente == null)
            return Result<int>.Failure("Cliente non trovato");
        
        // Check if already assigned
        var giaAssegnato = await _context.CouponsAssegnati
            .AnyAsync(ca => ca.CouponId == request.CouponId && ca.ClienteId == request.ClienteId && !ca.Utilizzato, cancellationToken);
        
        if (giaAssegnato)
            return Result<int>.Failure("Coupon già assegnato a questo cliente");
        
        var couponAssegnato = new CouponAssegnato
        {
            CouponId = request.CouponId,
            ClienteId = request.ClienteId,
            DataAssegnazione = DateTime.UtcNow,
            Utilizzato = false
        };
        
        _context.CouponsAssegnati.Add(couponAssegnato);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result<int>.Success(couponAssegnato.Id);
    }
}
