using AutoMapper;
using Fidelity.Infrastructure.Persistence;
using Fidelity.Application.Common.Interfaces;
using Fidelity.Domain.Entities;
using Fidelity.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Server.Services
{
    public class TransazioneService : ITransazioneService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        private const decimal PUNTI_PER_EURO = 0.1m; // 1 punto ogni 10€

        public TransazioneService(ApplicationDbContext context, IEmailService emailService, IMapper mapper)
        {
            _context = context;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<TransazioneResponse> AssegnaPuntiAsync(AssegnaPuntiRequest request, int puntoVenditaId, int responsabileId, string responsabileUsername)
        {
            var cliente = await _context.Clienti
                .Include(c => c.PuntoVenditaRegistrazione)
                .FirstOrDefaultAsync(c => c.CodiceFidelity == request.CodiceFidelity && c.Attivo);

            if (cliente == null)
            {
                throw new KeyNotFoundException($"Cliente con codice '{request.CodiceFidelity}' non trovato.");
            }

            int puntiDaAssegnare = (int)(request.ImportoSpesa * PUNTI_PER_EURO);

            if (puntiDaAssegnare < 1)
            {
                throw new ArgumentException("L'importo è troppo basso per assegnare punti (minimo 10€).");
            }

            var transazione = new Transazione
            {
                ClienteId = cliente.Id,
                PuntoVenditaId = puntoVenditaId > 0 ? puntoVenditaId : (cliente.PuntoVenditaRegistrazioneId ?? 0),
                ResponsabileId = responsabileId,
                PuntiAssegnati = puntiDaAssegnare,
                ImportoSpesa = request.ImportoSpesa,
                DataTransazione = DateTime.UtcNow,
                Tipo = TipoTransazione.Accumulo,
                Note = request.Note ?? $"Acquisto di {request.ImportoSpesa:C}"
            };

            _context.Transazioni.Add(transazione);
            cliente.PuntiTotali += puntiDaAssegnare;

            await _context.SaveChangesAsync();

            await _context.Entry(transazione).Reference(t => t.PuntoVendita).LoadAsync();
            await _context.Entry(transazione).Reference(t => t.Responsabile).LoadAsync();

            var response = _mapper.Map<TransazioneResponse>(transazione);

            // Fire and Forget Email
            _ = Task.Run(async () =>
                await _emailService.InviaEmailPuntiAssegnatiAsync(
                    cliente.Email,
                    cliente.Nome,
                    puntiDaAssegnare,
                    cliente.PuntiTotali,
                    request.ImportoSpesa));

            return response;
        }

        public async Task<ClienteDettaglioResponse?> GetClienteDettaglioAsync(string codiceFidelity)
        {
            var cliente = await _context.Clienti
                .Include(c => c.PuntoVenditaRegistrazione)
                .FirstOrDefaultAsync(c => c.CodiceFidelity == codiceFidelity && c.Attivo);

            if (cliente == null)
            {
                 return null;
            }

            var transazioni = await _context.Transazioni
                .Where(t => t.ClienteId == cliente.Id)
                .OrderByDescending(t => t.DataTransazione)
                .Take(10)
                .Include(t => t.PuntoVendita)
                .Include(t => t.Responsabile)
                .ToListAsync();

            var response = _mapper.Map<ClienteDettaglioResponse>(cliente);
            response.UltimeTransazioni = _mapper.Map<List<TransazioneResponse>>(transazioni);

            return response;
        }

        public async Task<List<TransazioneResponse>> GetStoricoAsync(int puntoVenditaId, int? clienteId = null, DateTime? dataInizio = null, DateTime? dataFine = null, int limit = 50)
        {
            var query = _context.Transazioni
                .Include(t => t.Cliente)
                .Include(t => t.PuntoVendita)
                .Include(t => t.Responsabile)
                .AsQueryable();

            if (puntoVenditaId > 0)
            {
                query = query.Where(t => t.PuntoVenditaId == puntoVenditaId);
            }

            if (clienteId.HasValue)
                query = query.Where(t => t.ClienteId == clienteId.Value);

            if (dataInizio.HasValue)
                query = query.Where(t => t.DataTransazione >= dataInizio.Value);

            if (dataFine.HasValue)
                query = query.Where(t => t.DataTransazione <= dataFine.Value);

             var transazioni = await query
                .OrderByDescending(t => t.DataTransazione)
                .Take(limit)
                .ToListAsync();

            return _mapper.Map<List<TransazioneResponse>>(transazioni);
        }
    }
}
