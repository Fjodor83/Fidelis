// Fidelity.Shared/DTOs/StatistichePuntoVenditaResponse.cs
namespace Fidelity.Shared.DTOs
{
    public class StatistichePuntoVenditaResponse
    {
        public int TotaleClienti { get; set; }
        public int ClientiRegistratiOggi { get; set; }
        public int ClientiRegistratiMeseCorrente { get; set; }
        public int TotalePuntiErogati { get; set; }
        public int PuntiErogatiOggi { get; set; }
        public decimal MediaPuntiPerCliente { get; set; }
    }
}