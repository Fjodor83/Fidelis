using Fidelity.Application.Common.Interfaces;

namespace Fidelity.Application.Common.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}
