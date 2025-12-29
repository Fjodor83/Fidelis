using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.Common.Models;
using Fidelity.Domain.Entities;
using Fidelity.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fidelity.Application.Clienti.Commands.RegistraCliente;

/// <summary>
/// Gestisce la registrazione completa di un nuovo cliente con validazione email e generazione card
/// </summary>
public class RegistraClienteCommandHandler : IRequestHandler<RegistraClienteCommand, Result<RegistraClienteResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ICardGeneratorService _cardGenerator;
    private readonly ILogger<RegistraClienteCommandHandler> _logger;

    public RegistraClienteCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        ICardGeneratorService cardGenerator,
        ILogger<RegistraClienteCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _cardGenerator = cardGenerator;
        _logger = logger;
    }

    public async Task<Result<RegistraClienteResponse>> Handle(
        RegistraClienteCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            Cliente? cliente = null;

            // FASE 2: Verifica Carta Esistente (se fornita)
            if (!string.IsNullOrEmpty(request.ExistingFidelityCode))
            {
                cliente = await AttivaCardEsistente(request, cancellationToken);
                if (cliente == null)
                {
                    return Result<RegistraClienteResponse>.Failure("Codice fedeltà non trovato o dati non corrispondenti");
                }
            }
            else
            {
                // FASE 1: Validazione Esistenza Email (Solo per nuovi utenti)
                if (await _context.Clienti.AnyAsync(c => c.Email == request.Email, cancellationToken))
                {
                    return Result<RegistraClienteResponse>.Failure("Email già registrata nel sistema");
                }

                // FASE 3: Crea Nuovo Cliente
                cliente = await CreaNuovoCliente(request, cancellationToken);
            }

            // FASE 4: Genera e Invia Card Digitale
            // Eseguiamo l'invio in modo sicuro attendendo il completamento per evitare ObjectDisposedException sul Context
            // o servizi non disponibili se la richiesta HTTP termina prematuramente.
            await InviaCardDigitaleAsync(cliente, cancellationToken);

            // FASE 5: Risposta
            return Result<RegistraClienteResponse>.Success(new RegistraClienteResponse
            {
                ClienteId = cliente.Id,
                CodiceFidelity = cliente.CodiceFidelity,
                Email = cliente.Email,
                Nome = cliente.Nome,
                Cognome = cliente.Cognome,
                PuntiTotali = cliente.PuntiTotali,
                Message = "Registrazione completata! Controlla la tua email per la card digitale."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la registrazione del cliente {Email}", request.Email);
            return Result<RegistraClienteResponse>.Failure("Si è verificato un errore imprevisto durante la registrazione.");
        }
    }

    private async Task<Cliente?> AttivaCardEsistente(
        RegistraClienteCommand request, 
        CancellationToken cancellationToken)
    {
        var cliente = await _context.Clienti
            .FirstOrDefaultAsync(c => 
                c.CodiceFidelity == request.ExistingFidelityCode && 
                c.Attivo, 
                cancellationToken);

        if (cliente == null || !string.IsNullOrEmpty(cliente.PasswordHash))
        {
            return null; // Card non trovata o già attivata online
        }

        // Verifica corrispondenza email (opzionale, basata sulla logica di business fornita)
        if (!cliente.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Tentativo di attivazione carta {Code} con email non corrispondente", request.ExistingFidelityCode);
            return null;
        }

        // Completa l'attivazione
        cliente.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        cliente.PrivacyAccettata = request.PrivacyAccepted;
        cliente.Telefono = request.Telefono;
        cliente.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Carta fedeltà {Code} attivata online per cliente {Email}", cliente.CodiceFidelity, cliente.Email);

        return cliente;
    }

    private async Task<Cliente> CreaNuovoCliente(
        RegistraClienteCommand request, 
        CancellationToken cancellationToken)
    {
        var codiceFidelity = await GeneraCodiceFidelityUnivocoAsync(cancellationToken);

        var cliente = new Cliente
        {
            Nome = request.Nome,
            Cognome = request.Cognome,
            Email = request.Email,
            Telefono = request.Telefono,
            DataRegistrazione = DateTime.UtcNow,
            PuntoVenditaRegistrazioneId = request.PuntoVenditaId,
            ResponsabileRegistrazioneId = null, // Registrazione online
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PrivacyAccettata = request.PrivacyAccepted,
            Attivo = true
        };

        // Usa il metodo della domain entity per impostare il codice fidelity (e scatenare l'evento)
        cliente.SetCodiceFidelity(CodiceFidelity.Create(codiceFidelity));

        _context.Clienti.Add(cliente);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Nuovo cliente registrato: {Code} - {Email}", cliente.CodiceFidelity, cliente.Email);

        return cliente;
    }

    private async Task<string> GeneraCodiceFidelityUnivocoAsync(CancellationToken cancellationToken)
    {
        const int maxAttempts = 10;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var numero = Random.Shared.Next(100000000, 999999999);
            var codice = $"SUN{numero}";

            if (!await _context.Clienti.AnyAsync(c => c.CodiceFidelity == codice, cancellationToken))
            {
                return codice;
            }
        }

        throw new InvalidOperationException("Impossibile generare un codice fedeltà univoco dopo diversi tentativi.");
    }

    private async Task InviaCardDigitaleAsync(Cliente cliente, CancellationToken cancellationToken)
    {
        try
        {
            // Carica il punto vendita per la generazione della card (se presente)
            PuntoVendita? pv = null;
            if (cliente.PuntoVenditaRegistrazioneId.HasValue)
            {
                pv = await _context.PuntiVendita.FindAsync(new object[] { cliente.PuntoVenditaRegistrazioneId.Value }, cancellationToken);
            }

            // Genera l'immagine della card
            var cardImage = await _cardGenerator.GeneraCardDigitaleAsync(cliente, pv);

            // Invia l'email di benvenuto
            await _emailService.InviaEmailBenvenutoAsync(
                cliente.Email,
                cliente.Nome,
                cliente.CodiceFidelity,
                cardImage
            );

            _logger.LogInformation("Card digitale inviata con successo a {Email}", cliente.Email);
        }
        catch (Exception ex)
        {
            // L'errore nell'invio email non deve annullare la registrazione, ma va loggato
            _logger.LogError(ex, "Errore durante l'invio della card digitale a {Email}", cliente.Email);
        }
    }
}
