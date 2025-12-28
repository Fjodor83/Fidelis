using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.Common.Models;
using Fidelity.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Application.Clienti.Commands.RegistraCliente;

public class RegistraClienteCommandHandler : IRequestHandler<RegistraClienteCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public RegistraClienteCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(RegistraClienteCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (await _context.Clienti.AnyAsync(c => c.Email == request.Email, cancellationToken))
        {
            return Result<int>.Failure("Email già registrata");
        }

        // Generate unique fidelity code
        var codiceFidelity = await GenerateUniqueCodeAsync(cancellationToken);

        // Create new cliente
        var cliente = new Cliente
        {
            CodiceFidelity = codiceFidelity,
            Nome = request.Nome,
            Cognome = request.Cognome,
            Email = request.Email,
            Telefono = request.Telefono,
            DataRegistrazione = DateTime.UtcNow,
            PuntoVenditaRegistrazioneId = request.PuntoVenditaId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PrivacyAccettata = request.PrivacyAccepted,
            Attivo = true,
            PuntiTotali = 0
        };

        _context.Clienti.Add(cliente);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(cliente.Id);
    }

    private async Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken)
    {
        string codice;
        bool exists;
        
        do
        {
            var numero = new Random().Next(100000000, 999999999);
            codice = $"SUN{numero}";
            exists = await _context.Clienti.AnyAsync(c => c.CodiceFidelity == codice, cancellationToken);
        }
        while (exists);

        return codice;
    }
}
