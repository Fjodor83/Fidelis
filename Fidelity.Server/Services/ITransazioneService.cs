using Fidelity.Shared.DTOs;
using Fidelity.Shared.Models;

namespace Fidelity.Server.Services
{
    public interface ITransazioneService
    {
        Task<TransazioneResponse> AssegnaPuntiAsync(AssegnaPuntiRequest request, int puntoVenditaId, int responsabileId, string responsabileUsername);
        Task<ClienteDettaglioResponse> GetClienteDettaglioAsync(string codiceFidelity);
        Task<List<TransazioneResponse>> GetStoricoAsync(int puntoVenditaId, int? clienteId = null, DateTime? dataInizio = null, DateTime? dataFine = null, int limit = 50);
    }
}
