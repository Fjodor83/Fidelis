using Fidelity.Domain.Common;

namespace Fidelity.Domain.Entities;

public class Cliente : BaseEntity
{
    public string CodiceFidelity { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Cognome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public DateTime DataRegistrazione { get; set; }
    
    // Punto vendita origine
    public int? PuntoVenditaRegistrazioneId { get; set; }
    
    // Responsabile registrazione
    public int? ResponsabileRegistrazioneId { get; set; }
    
    // Fidelity points
    public int PuntiTotali { get; set; } = 0;
    
    // Status
    public bool Attivo { get; set; } = true;
    
    // Security
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? AccountLockedUntil { get; set; }
    
    // Privacy
    public bool PrivacyAccettata { get; set; }
    
    // Authentication
    public string? PasswordHash { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    
    // Business logic methods
    public void AggiungiPunti(int punti)
    {
        if (punti <= 0)
            throw new ArgumentException("I punti devono essere maggiori di zero", nameof(punti));
        
        PuntiTotali += punti;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SottraiPunti(int punti)
    {
        if (punti <= 0)
            throw new ArgumentException("I punti devono essere maggiori di zero", nameof(punti));
        
        if (PuntiTotali < punti)
            throw new InvalidOperationException($"Punti insufficienti. Disponibili: {PuntiTotali}, Richiesti: {punti}");
        
        PuntiTotali -= punti;
        UpdatedAt = DateTime.UtcNow;
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
}
