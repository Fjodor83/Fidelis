namespace Fidelity.Application.DTOs;

public class ClienteDto
{
    public int Id { get; set; }
    public string CodiceFidelity { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Cognome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public DateTime DataRegistrazione { get; set; }
    public int PuntiTotali { get; set; }
    public bool Attivo { get; set; }
}
