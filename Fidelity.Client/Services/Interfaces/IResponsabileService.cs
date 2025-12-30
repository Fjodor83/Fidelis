using Fidelity.Shared.DTOs;

namespace Fidelity.Client.Services.Interfaces;

public interface IResponsabileService
{
    Task<bool> CambiaPasswordAsync(CambiaPasswordRequest request);
}
