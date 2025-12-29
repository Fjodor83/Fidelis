using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.Common.Models;
using MediatR;

namespace Fidelity.Application.Coupons.Commands.DeleteCoupon;

public class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteCouponCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _context.Coupons.FindAsync(new object[] { request.Id }, cancellationToken);

        if (coupon == null)
            return Result.Failure("Coupon non trovato.");

        // Soft delete
        coupon.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
