using Fidelity.Application.Common.Interfaces;
using Fidelity.Domain.Entities;

namespace Fidelity.Infrastructure.Persistence.Repositories;

public class CouponRepository : Repository<Coupon>, ICouponRepository
{
    public CouponRepository(ApplicationDbContext context) : base(context)
    {
    }
}
