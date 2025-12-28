using System.ComponentModel.DataAnnotations;

namespace Fidelity.Shared.Models;

public class RefreshToken
{
    public int Id { get; set; }
    
    [Required]
    public string Token { get; set; } = string.Empty;
    
    [Required]
    public string JwtId { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; }
    
    public DateTime ExpiryDate { get; set; }
    
    public bool IsUsed { get; set; }
    
    public bool IsRevoked { get; set; }
    
    // Cliente (optional)
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    
    // Responsabile (optional)
    public int? ResponsabileId { get; set; }
    public Responsabile? Responsabile { get; set; }
}
