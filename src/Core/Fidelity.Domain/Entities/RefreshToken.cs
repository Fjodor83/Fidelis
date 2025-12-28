using Fidelity.Domain.Common;

namespace Fidelity.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public required string Token { get; set; }
    public required string JwtId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedDate { get; set; }
    
    // Navigation
    public int? ClienteId { get; set; }
    public int? ResponsabileId { get; set; }
    
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
