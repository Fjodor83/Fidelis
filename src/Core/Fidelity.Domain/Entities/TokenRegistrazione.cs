using System.Security.Cryptography;
using Fidelity.Domain.Common;

namespace Fidelity.Domain.Entities;

/// <summary>
/// Token for email-based registration flow
/// ISO 25000: Security, Reliability
/// </summary>
public class TokenRegistrazione : BaseEntity
{
    private const int TokenLength = 16;
    private const int DefaultExpirationHours = 24; // Extended from 15 minutes

    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;

    // Context
    public int PuntoVenditaId { get; set; }
    public int ResponsabileId { get; set; }

    // Lifecycle
    public DateTime DataCreazione { get; set; }
    public DateTime DataScadenza { get; set; }
    public bool Utilizzato { get; set; } = false;
    public DateTime? DataUtilizzo { get; set; }

    // Navigation properties
    public virtual PuntoVendita PuntoVendita { get; set; } = null!;
    public virtual Responsabile Responsabile { get; set; } = null!;

    // Factory method
    public static TokenRegistrazione Create(string email, int puntoVenditaId, int responsabileId, int expirationHours = DefaultExpirationHours)
    {
        return new TokenRegistrazione
        {
            Email = email.ToLowerInvariant().Trim(),
            Token = GenerateSecureToken(),
            PuntoVenditaId = puntoVenditaId,
            ResponsabileId = responsabileId,
            DataCreazione = DateTime.UtcNow,
            DataScadenza = DateTime.UtcNow.AddHours(expirationHours),
            Utilizzato = false
        };
    }

    /// <summary>
    /// Generate cryptographically secure token
    /// </summary>
    private static string GenerateSecureToken()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var result = new char[TokenLength];
        var bytes = new byte[TokenLength];

        RandomNumberGenerator.Fill(bytes);

        for (int i = 0; i < TokenLength; i++)
        {
            result[i] = chars[bytes[i] % chars.Length];
        }

        return new string(result);
    }

    // Business methods
    public bool IsValido()
    {
        return !Utilizzato && DateTime.UtcNow <= DataScadenza;
    }

    public bool IsScaduto()
    {
        return DateTime.UtcNow > DataScadenza;
    }

    public void SegnaUtilizzato()
    {
        if (Utilizzato)
            throw new InvalidOperationException("Token già utilizzato");

        if (IsScaduto())
            throw new InvalidOperationException("Token scaduto");

        Utilizzato = true;
        DataUtilizzo = DateTime.UtcNow;
    }

    public TimeSpan TempoRimanente()
    {
        var remaining = DataScadenza - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }
}
