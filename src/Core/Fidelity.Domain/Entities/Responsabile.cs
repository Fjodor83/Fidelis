using Fidelity.Domain.Common;

namespace Fidelity.Domain.Entities;

/// <summary>
/// Responsabile entity - Store manager/staff
/// ISO 25000: Security, Accountability
/// </summary>
public class Responsabile : SoftDeleteEntity
{
    // Identity
    public string Username { get; set; } = string.Empty;
    public string? NomeCompleto { get; set; }
    public string? Email { get; set; }

    // Security
    public string PasswordHash { get; set; } = string.Empty;
    public string Ruolo { get; set; } = "Responsabile"; // "Admin", "Responsabile"
    public bool Attivo { get; set; } = true;

    // Account security
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? AccountLockedUntil { get; set; }
    public bool RichiestaResetPassword { get; set; } = false;

    // Audit
    public DateTime? UltimoAccesso { get; set; }
    public string? UltimoAccessoIP { get; set; }

    // Navigation properties
    public virtual ICollection<ResponsabilePuntoVendita> ResponsabilePuntiVendita { get; set; } = new List<ResponsabilePuntoVendita>();
    public virtual ICollection<Transazione> Transazioni { get; set; } = new List<Transazione>();
    public virtual ICollection<TokenRegistrazione> TokenRegistrazioniCreati { get; set; } = new List<TokenRegistrazione>();

    // Business methods
    public bool IsAccountLocked()
    {
        return AccountLockedUntil.HasValue && AccountLockedUntil.Value > DateTime.UtcNow;
    }

    public void BloccaAccount(int minuti = 15)
    {
        AccountLockedUntil = DateTime.UtcNow.AddMinutes(minuti);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SbloccaAccount()
    {
        FailedLoginAttempts = 0;
        AccountLockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RegistraAccesso(string? ipAddress = null)
    {
        UltimoAccesso = DateTime.UtcNow;
        UltimoAccessoIP = ipAddress;
        FailedLoginAttempts = 0;
        AccountLockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementaLoginFallito(int maxTentativi = 5, int minutiBlocco = 15)
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= maxTentativi)
        {
            BloccaAccount(minutiBlocco);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsAdmin() => Ruolo == "Admin";
}
