namespace Fidelity.Application.DTOs;

public class CouponDto
{
    public int Id { get; set; }
    public string Codice { get; set; } = string.Empty;
    public string Titolo { get; set; } = string.Empty;
    public string? Descrizione { get; set; }
    public decimal ValoreSconto { get; set; }
    public string TipoSconto { get; set; } = string.Empty;
    public DateTime DataInizio { get; set; }
    public DateTime DataScadenza { get; set; }
    public bool Attivo { get; set; }
    public bool IsValido { get; set; }
}

public class CouponAssegnatoDto
{
    public int Id { get; set; }
    public int CouponId { get; set; }
    public CouponDto Coupon { get; set; } = new();
    public int ClienteId { get; set; }
    public DateTime DataAssegnazione { get; set; }
    public DateTime? DataUtilizzo { get; set; }
    public bool Utilizzato { get; set; }
}
