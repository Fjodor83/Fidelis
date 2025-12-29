using Fidelity.Domain.Common;
using Fidelity.Domain.ValueObjects;
using Fidelity.Domain.Events;

namespace Fidelity.Domain.Entities;

/// <summary>
/// Cliente entity - Core domain entity for loyalty program
/// ISO 25000: Functional Suitability, Security
/// </summary>
public class Cliente : SoftDeleteEntity
{
    // Identity
    public string CodiceFidelity { get; set; } = string.Empty;

    // Personal data
    public string Nome { get; set; } = string.Empty;
    public string Cognome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }

    // Registration tracking
    public DateTime DataRegistrazione { get; set; }
    public int? PuntoVenditaRegistrazioneId { get; set; }
    public int? ResponsabileRegistrazioneId { get; set; }

    // Fidelity program
    public int PuntiTotali { get; set; } = 0;
    public int PuntiSpesi { get; set; } = 0;
    public LivelloFedelta Livello { get; set; } = LivelloFedelta.Bronze;

    // Status
    public bool Attivo { get; set; } = true;

    // Security
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? AccountLockedUntil { get; set; }
    public string? PasswordHash { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }

    // Privacy
    public bool PrivacyAccettata { get; set; }
    public DateTime? PrivacyAccettataData { get; set; }

    // Navigation properties
    public virtual PuntoVendita? PuntoVenditaRegistrazione { get; set; }
    public virtual Responsabile? ResponsabileRegistrazione { get; set; }
    public virtual ICollection<Transazione> Transazioni { get; set; } = new List<Transazione>();
    public virtual ICollection<CouponAssegnato> CouponAssegnati { get; set; } = new List<CouponAssegnato>();

    // Computed properties
    public string NomeCompleto => $"{Nome} {Cognome}";
    public int PuntiDisponibili => PuntiTotali - PuntiSpesi;

    // Business logic methods
    public void SetCodiceFidelity(CodiceFidelity codice)
    {
        if (string.IsNullOrEmpty(CodiceFidelity))
        {
            CodiceFidelity = codice.Value;
            AddDomainEvent(new ClienteRegistratoEvent(Id, codice.Value, Email));
        }
    }

    public void AggiungiPunti(int punti, int transazioneId)
    {
        if (punti <= 0)
            throw new ArgumentException("I punti devono essere maggiori di zero", nameof(punti));

        PuntiTotali += punti;
        UpdatedAt = DateTime.UtcNow;

        // Check level upgrade
        AggiornaLivello();

        AddDomainEvent(new PuntiAggiuntiEvent(Id, punti, PuntiTotali, transazioneId));
    }

    public void SpindiPunti(int punti)
    {
        if (punti <= 0)
            throw new ArgumentException("I punti devono essere maggiori di zero", nameof(punti));

        if (PuntiDisponibili < punti)
            throw new InvalidOperationException($"Punti insufficienti. Disponibili: {PuntiDisponibili}, Richiesti: {punti}");

        PuntiSpesi += punti;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PuntiSpesiEvent(Id, punti, PuntiDisponibili));
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

    public bool IsAccountLocked()
    {
        return AccountLockedUntil.HasValue && AccountLockedUntil.Value > DateTime.UtcNow;
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

    private void AggiornaLivello()
    {
        var nuovoLivello = PuntiTotali switch
        {
            >= 5000 => LivelloFedelta.Platinum,
            >= 2000 => LivelloFedelta.Gold,
            >= 500 => LivelloFedelta.Silver,
            _ => LivelloFedelta.Bronze
        };

        if (nuovoLivello != Livello)
        {
            var vecchioLivello = Livello;
            Livello = nuovoLivello;
            AddDomainEvent(new LivelloFedeltaCambiatoEvent(Id, vecchioLivello, nuovoLivello));
        }
    }

    public void AccettaPrivacy()
    {
        PrivacyAccettata = true;
        PrivacyAccettataData = DateTime.UtcNow;
    }
}

public enum LivelloFedelta
{
    Bronze = 0,
    Silver = 1,
    Gold = 2,
    Platinum = 3
}
