// Fidelity.Server/Services/ICardGeneratorService.cs
using System.Threading.Tasks;
using Fidelity.Shared.Models;

namespace Fidelity.Server.Services
{
    public interface ICardGeneratorService
    {
        Task<byte[]> GeneraCardDigitaleAsync(Cliente cliente, PuntoVendita puntoVendita);
    }
}