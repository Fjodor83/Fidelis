using Fidelity.Domain.Entities;

namespace Fidelity.Application.Common.Interfaces;

/// <summary>
/// Card generator service interface
/// </summary>
public interface ICardGeneratorService
{
    Task<byte[]> GeneraCardDigitaleAsync(Cliente cliente, PuntoVendita? puntoVendita);
    Task<byte[]> GeneraQRCodeAsync(string contenuto, int dimensione = 200);
}
