using Fidelity.Application.Common.Models;
using Fidelity.Application.DTOs;
using MediatR;

namespace Fidelity.Application.Auth.Commands.Login;

public record LoginClienteCommand : IRequest<Result<LoginResponseDto>>
{
    public string EmailOrCode { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
