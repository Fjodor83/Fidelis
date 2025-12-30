using System.ComponentModel.DataAnnotations;

namespace Fidelity.Domain.Entities;

public class IdempotencyRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RequestHash { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
