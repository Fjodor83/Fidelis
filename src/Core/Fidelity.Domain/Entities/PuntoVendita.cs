using Fidelity.Domain.Common;

namespace Fidelity.Domain.Entities;

/// <summary>
/// Punto Vendita entity - Store location
/// ISO 25000: Functional Suitability
/// </summary>
public class PuntoVendita : SoftDeleteEntity
{
    public string Codice { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Indirizzo { get; set; }
    public string? Citta { get; set; }
    public string? CAP { get; set; }
    public string? Provincia { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }

    // Business settings
    public decimal PuntiPerEuro { get; set; } = 0.1m; // Default: 1 punto ogni 10€
    public bool Attivo { get; set; } = true;

    // Opening hours (JSON serialized)
    public string? OrariApertura { get; set; }

    // Navigation properties
    public virtual ICollection<ResponsabilePuntoVendita> ResponsabilePuntiVendita { get; set; } = new List<ResponsabilePuntoVendita>();
    public virtual ICollection<Cliente> ClientiRegistrati { get; set; } = new List<Cliente>();
    public virtual ICollection<Transazione> Transazioni { get; set; } = new List<Transazione>();

    // Business methods
    public int CalcolaPunti(decimal importo)
    {
        if (importo <= 0) return 0;
        return (int)Math.Floor(importo * PuntiPerEuro);
    }

    public string IndirizzoCompleto => string.Join(", ",
        new[] { Indirizzo, CAP, Citta, Provincia }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
}
