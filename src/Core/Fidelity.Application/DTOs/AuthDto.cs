namespace Fidelity.Application.DTOs;

public class LoginResponseDto
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? Messaggio { get; set; }
    public ClienteDto? Cliente { get; set; }
}

public class DashboardStatsDto
{
    public int TotaleClienti { get; set; }
    public int ClientiAttivi { get; set; }
    public int TotalePuntiDistribuiti { get; set; }
    public decimal TotaleTransazioniMese { get; set; }
    public int NumeroTransazioniMese { get; set; }
    public int CouponsAttivi { get; set; }
}
