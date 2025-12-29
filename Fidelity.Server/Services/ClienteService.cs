using AutoMapper;
using Fidelity.Infrastructure.Persistence;
using Fidelity.Domain.Entities;
using Fidelity.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Server.Services
{
    public class ClienteService : IClienteService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ClienteService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ClienteResponse>> CercaClientiAsync(string query, string? userRole, int? userPuntoVenditaId)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
            {
                 throw new ArgumentException("Query di ricerca troppo breve. Minimo 3 caratteri.");
            }

            var clienti = await _context.Clienti
                .Include(c => c.PuntoVenditaRegistrazione)
                .Where(c => 
                    (c.CodiceFidelity.Contains(query) || 
                        c.Email.Contains(query) ||
                        c.Nome.Contains(query) ||
                        c.Cognome.Contains(query)) &&
                    (userRole == "Admin" || (userPuntoVenditaId.HasValue && c.PuntoVenditaRegistrazioneId == userPuntoVenditaId.Value)))
                .Take(10)
                .ToListAsync();

            return _mapper.Map<List<ClienteResponse>>(clienti);
        }

        public async Task<List<ClienteResponse>> GetClientiByPuntoVenditaAsync(string? userRole, int? userPuntoVenditaId)
        {
            if (userRole != "Admin" && !userPuntoVenditaId.HasValue)
            {
                throw new UnauthorizedAccessException("Punto vendita non trovato.");
            }

            IQueryable<Cliente> query = _context.Clienti
                .Include(c => c.PuntoVenditaRegistrazione);

            if (userRole != "Admin")
            {
                query = query.Where(c => c.PuntoVenditaRegistrazioneId == userPuntoVenditaId!.Value);
            }

            var clienti = await query
                .Where(c => c.Attivo)
                .OrderByDescending(c => c.DataRegistrazione)
                .ToListAsync();

            return _mapper.Map<List<ClienteResponse>>(clienti);
        }

        public async Task<ClienteResponse?> GetClienteByIdAsync(int id, string? userRole, int? userPuntoVenditaId)
        {
            var cliente = await _context.Clienti
                .Include(c => c.PuntoVenditaRegistrazione)
                .Where(c => c.Id == id &&
                    (userRole == "Admin" || (userPuntoVenditaId.HasValue && c.PuntoVenditaRegistrazioneId == userPuntoVenditaId.Value)))
                .FirstOrDefaultAsync();

            if (cliente == null) return null;

            return _mapper.Map<ClienteResponse>(cliente);
        }
    }
}
