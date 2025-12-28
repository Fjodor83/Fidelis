using System;
using System.Collections.Generic;

namespace Fidelity.Shared.DTOs
{
    public class DashboardStatsDTO
    {
        public int TotaleClienti { get; set; }
        public int ClientiRegistratiOggi { get; set; }
        public int PuntiTotaliEmessi { get; set; }
        public int CouponAttivi { get; set; }
        public int CouponRiscattati { get; set; }
        public int TransazioniOggi { get; set; }
    }

    public class RegistrationStatsDTO
    {
        public DateTime Data { get; set; }
        public int Count { get; set; }
    }

    public class RecentActivityDTO
    {
        public string Tipo { get; set; } // "Punti" o "Coupon"
        public string ClienteNome { get; set; }
        public string Descrizione { get; set; }
        public DateTime Data { get; set; }
        public string PuntoVendita { get; set; }
        public string Responsabile { get; set; }
    }
}
