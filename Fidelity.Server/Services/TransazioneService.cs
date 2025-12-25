using AutoMapper;
using Fidelity.Server.Data;
using Fidelity.Shared.DTOs;
using Fidelity.Shared.Models;
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

            // If Responsabile is "Admin", PuntoVendita might be 0 or passed explicitly.
            // Logic handled by caller for ID, but here we enforce relation.
            // Actually, controller passed explicit puntoVenditaId strategy.

            var transazione = new Transazione
            {
                ClienteId = cliente.Id,
                PuntoVenditaId = puntoVenditaId > 0 ? puntoVenditaId : (cliente.PuntoVenditaRegistrazioneId ?? 0),
                ResponsabileId = responsabileId,
                PuntiAssegnati = puntiDaAssegnare,
                ImportoSpesa = request.ImportoSpesa,
                DataTransazione = DateTime.UtcNow,
                TipoTransazione = "Accumulo",
                Note = request.Note ?? $"Acquisto di {request.ImportoSpesa:C}"
            };

            _context.Transazioni.Add(transazione);
            cliente.PuntiTotali += puntiDaAssegnare;

            await _context.SaveChangesAsync();

            await _context.Entry(transazione).Reference(t => t.PuntoVendita).LoadAsync();
            
            // Should load Responsabile too if needed for Response mapping
            // But ResponsabileId is set.
            // Only if response needs Responsabile Name. MappingProfile uses it?
            // Yes: .ForMember(dest => dest.ResponsabileNome, opt => opt.MapFrom(src => src.Responsabile.Username))
            // So we need to load it or fake it.
            // If ID is set, EF typically doesn't auto-load prop unless requested.
            // But we can just reload it.
             await _context.Entry(transazione).Reference(t => t.Responsabile).LoadAsync();

            var response = _mapper.Map<TransazioneResponse>(transazione);

            // Fire and Forget Email
            _ = Task.Run(() =>
                _emailService.InviaEmailPuntiGuadagnatiAsync(
                    cliente.Email,
                    cliente.Nome,
                    puntiDaAssegnare,
                    cliente.PuntiTotali,
                    transazione.PuntoVendita.Nome));

            return response;
        }

        public async Task<ClienteDettaglioResponse> GetClienteDettaglioAsync(string codiceFidelity)
        {
            var cliente = await _context.Clienti
                .Include(c => c.PuntoVenditaRegistrazione)
                .FirstOrDefaultAsync(c => c.CodiceFidelity == codiceFidelity && c.Attivo);

            if (cliente == null)
            {
                 // Service returns null or throws? Controller returns NotFound if null.
                 // Returing null allows Controller to handle 404.
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

            // 0 or -1 could mean "All" for Admin, but usually we pass specific ID or handled by caller.
            // If passed 0, we assume no filter (Admin view) IF logic allows.
            // Logic: if puntoVenditaId > 0, filter.
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
