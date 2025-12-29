using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.Common.Models;
using Fidelity.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Fidelity.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Fidelity.Application.Transazioni.Commands.RegistraTransazione;

public class RegistraTransazioneCommandHandler : IRequestHandler<RegistraTransazioneCommand, Result<TransazioneDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<RegistraTransazioneCommandHandler> _logger;

    public RegistraTransazioneCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        ILogger<RegistraTransazioneCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<TransazioneDto>> Handle(RegistraTransazioneCommand request, CancellationToken cancellationToken)
    {
        // Verify cliente exists
        var cliente = await _context.Clienti
            .Include(c => c.PuntoVenditaRegistrazione)
            .FirstOrDefaultAsync(c => c.Id == request.ClienteId, cancellationToken);
        
        if (cliente == null)
            return Result<TransazioneDto>.Failure($"Cliente con ID {request.ClienteId} non trovato");
        
        if (!cliente.Attivo)
            return Result<TransazioneDto>.Failure("Il cliente non è attivo");
        
        var puntoVendita = await _context.PuntiVendita.FindAsync(new object[] { request.PuntoVenditaId }, cancellationToken);
         if (puntoVendita == null) return Result<TransazioneDto>.Failure($"Punto vendita {request.PuntoVenditaId} non trovato");

        // Calculate points
        var puntiGuadagnati = Transazione.CalcolaPunti(request.Importo);
        
        // Create transaction
        var transazione = new Transazione
        {
            ClienteId = request.ClienteId,
            PuntoVenditaId = request.PuntoVenditaId,
            ResponsabileId = request.ResponsabileId,
            DataTransazione = DateTime.UtcNow,
            ImportoSpesa = request.Importo,
            PuntiAssegnati = puntiGuadagnati,
            Note = request.Note
        };
        
        // Validate business rules
        try
        {
            transazione.Validate();
        }
        catch (ArgumentException ex)
        {
            return Result<TransazioneDto>.Failure(ex.Message);
        }
        
        _context.Transazioni.Add(transazione);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Update cliente points using domain method
        cliente.AggiungiPunti(puntiGuadagnati, transazione.Id);
        await _context.SaveChangesAsync(cancellationToken);

        // Send email notification (fire and forget)
        if (!string.IsNullOrEmpty(cliente.Email))
        {
            var email = cliente.Email;
            var nome = cliente.Nome;
            var punti = puntiGuadagnati;
            var totale = cliente.PuntiTotali;
            var importo = request.Importo;

            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.InviaEmailPuntiAssegnatiAsync(email, nome, punti, totale, importo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore invio email punti assegnati a {Email}", email);
                }
            });
        }
        
        var dto = new TransazioneDto
        {
            Id = transazione.Id,
            ClienteId = cliente.Id,
            ClienteNome = $"{cliente.Nome} {cliente.Cognome}",
            CodiceFidelity = cliente.CodiceFidelity,
            PuntoVenditaNome = puntoVendita.Nome,
            DataTransazione = transazione.DataTransazione,
            Importo = transazione.ImportoSpesa,
            PuntiGuadagnati = transazione.PuntiAssegnati,
            Tipo = transazione.Tipo.ToString(),
            Note = transazione.Note
        };

        return Result<TransazioneDto>.Success(dto);
    }
}
