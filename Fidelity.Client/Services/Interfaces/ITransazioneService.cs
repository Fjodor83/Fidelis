using Fidelity.Application.DTOs;
using Fidelity.Shared.DTOs;

namespace Fidelity.Client.Services.Interfaces;

public interface ITransazioneService
{
    Task<bool> RegistraTransazione(TransazioneRequest request);
    Task<TransazioneDto?> AssegnaPuntiAsync(AssegnaPuntiRequest request);
    Task<List<TransazioneDto>> GetUltimiMovimenti(int clienteId);
}
