using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.Common.Models;
using Fidelity.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Application.Auth.Commands.Login;

public class LoginClienteCommandHandler : IRequestHandler<LoginClienteCommand, Result<LoginResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public LoginClienteCommandHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginClienteCommand request, CancellationToken cancellationToken)
    {
        // Find cliente by email or fidelity code
        var cliente = await _context.Clienti
            .FirstOrDefaultAsync(c => 
                (c.Email == request.EmailOrCode || c.CodiceFidelity == request.EmailOrCode) && 
                c.Attivo, cancellationToken);

        if (cliente == null)
        {
            return Result<LoginResponseDto>.Failure("Credenziali non valide");
        }

        // Check if account is locked
        if (cliente.IsAccountLocked())
        {
            return Result<LoginResponseDto>.Failure($"Account bloccato fino alle {cliente.AccountLockedUntil:HH:mm}");
        }

        // Check if password is set
        if (string.IsNullOrEmpty(cliente.PasswordHash))
        {
            return Result<LoginResponseDto>.Failure("Account non ancora attivato online. Procedi con la registrazione.");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, cliente.PasswordHash))
        {
            // Increment failed attempts
            cliente.FailedLoginAttempts++;
            
            if (cliente.FailedLoginAttempts >= 5)
            {
                cliente.BloccaAccount(15);
                await _context.SaveChangesAsync(cancellationToken);
                return Result<LoginResponseDto>.Failure("Troppi tentativi falliti. Account bloccato per 15 minuti.");
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            return Result<LoginResponseDto>.Failure("Credenziali non valide");
        }

        // Reset failed attempts on successful login
        cliente.SbloccaAccount();
        await _context.SaveChangesAsync(cancellationToken);

        // Generate tokens
        var (token, refreshToken) = await _jwtService.GenerateTokensAsync(cliente.Id, "Cliente", cancellationToken);

        var response = new LoginResponseDto
        {
            Success = true,
            Token = token,
            RefreshToken = refreshToken,
            Messaggio = "Login effettuato con successo",
            Cliente = new ClienteDto
            {
                Id = cliente.Id,
                CodiceFidelity = cliente.CodiceFidelity,
                Nome = cliente.Nome,
                Cognome = cliente.Cognome,
                Email = cliente.Email,
                PuntiTotali = cliente.PuntiTotali,
                Attivo = cliente.Attivo
            }
        };

        return Result<LoginResponseDto>.Success(response);
    }
}
