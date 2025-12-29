using Fidelity.Domain.Common;

namespace Fidelity.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public required string Token { get; set; }
    public required string JwtId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedDate { get; set; }
    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    
    // Navigation
    public int? ClienteId { get; set; }
    public virtual Cliente? Cliente { get; set; }
    public int? ResponsabileId { get; set; }
    public virtual Responsabile? Responsabile { get; set; }
    
    // Business logic
    public bool IsValid()
    {
        return !IsRevoked && DateTime.UtcNow < ExpiryDate;
    }
    
    public void Revoke()
    {
        IsRevoked = true;
        RevokedDate = DateTime.UtcNow;
    }
}
