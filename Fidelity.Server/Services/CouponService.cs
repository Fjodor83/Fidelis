using AutoMapper;
using Fidelity.Infrastructure.Persistence;
using Fidelity.Application.Common.Interfaces;
using Fidelity.Domain.Entities;
using Fidelity.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Server.Services
{
    public class CouponService : ICouponService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public CouponService(ApplicationDbContext context, IEmailService emailService, IMapper mapper)
        {
            _context = context;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<List<CouponDTO>> GetAllCouponsAsync()
        {
            var coupons = await _context.Coupons
                .OrderByDescending(c => c.DataInizio)
                .ToListAsync();

            return _mapper.Map<List<CouponDTO>>(coupons);
        }

        public async Task<CouponDTO?> GetCouponAsync(int id)
        {
             var c = await _context.Coupons.FindAsync(id);
             if (c == null) return null;
             return _mapper.Map<CouponDTO>(c);
        }

        public async Task<List<CouponDTO>> GetCouponsDisponibiliAsync()
        {
            var today = DateTime.UtcNow;
            var coupons = await _context.Coupons
                .Where(c => c.Attivo && c.DataScadenza > today)
                .OrderBy(c => c.DataScadenza)
                .ToListAsync();

            return _mapper.Map<List<CouponDTO>>(coupons);
        }

        public async Task<CouponDTO> CreateCouponAsync(CouponRequest request)
        {
            if (await _context.Coupons.AnyAsync(c => c.Codice == request.Codice))
                throw new ArgumentException("Esiste già un coupon con questo codice.");

            var coupon = new Coupon
            {
                Codice = request.Codice.ToUpper(),
                Titolo = request.Titolo,
                Descrizione = request.Descrizione,
                ValoreSconto = request.ValoreSconto,
                TipoSconto = Enum.TryParse<TipoSconto>(request.TipoSconto, true, out var tipo) ? tipo : TipoSconto.Percentuale,
                DataInizio = request.DataInizio,
                DataScadenza = request.DataScadenza,
                Attivo = request.Attivo
            };

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            // ✅ ASSEGNA IL COUPON A TUTTI I CLIENTI ATTIVI
            if (coupon.Attivo)
            {
                var clientiAttivi = await _context.Clienti
                    .Where(c => c.Attivo && !string.IsNullOrEmpty(c.Email))
                    .ToListAsync();

                foreach (var cliente in clientiAttivi)
                {
                    // ✅ CREA L'ASSEGNAZIONE
                    var assegnazione = new CouponAssegnato
                    {
                        CouponId = coupon.Id,
                        ClienteId = cliente.Id,
                        DataAssegnazione = DateTime.UtcNow,
                        Utilizzato = false,
                        Motivo = MotivoAssegnazione.Automatico
                    };
                    _context.CouponAssegnati.Add(assegnazione);

                    // Invia email
                    try
                    {
                        await _emailService.InviaEmailNuovoCouponAsync(
                            cliente.Email,
                            cliente.Nome,
                            coupon.Titolo,
                            coupon.Codice,
                            coupon.DataScadenza
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Errore invio email a {cliente.Email}: {ex.Message}");
                    }
                }

                // ✅ SALVA TUTTE LE ASSEGNAZIONI
                await _context.SaveChangesAsync();
            }

            return _mapper.Map<CouponDTO>(coupon);
        }

        public async Task<CouponDTO?> UpdateCouponAsync(int id, CouponRequest request)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return null;

            // Verifica codice univoco (escluso il coupon corrente)
            if (request.Codice != coupon.Codice && 
                await _context.Coupons.AnyAsync(c => c.Codice == request.Codice))
            {
                throw new ArgumentException("Esiste già un coupon con questo codice.");
            }

            // Aggiorna proprietà
            coupon.Codice = request.Codice.ToUpper();
            coupon.Titolo = request.Titolo;
            coupon.Descrizione = request.Descrizione;
            coupon.ValoreSconto = request.ValoreSconto;
            coupon.TipoSconto = Enum.TryParse<TipoSconto>(request.TipoSconto, true, out var tipo) ? tipo : TipoSconto.Percentuale;
            coupon.DataInizio = request.DataInizio;
            coupon.DataScadenza = request.DataScadenza;
            coupon.Attivo = request.Attivo;

            await _context.SaveChangesAsync();

            return _mapper.Map<CouponDTO>(coupon);
        }

        public async Task DeleteCouponAsync(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return; 

            try
            {
                _context.Coupons.Remove(coupon);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Soft delete fallback
                coupon.Attivo = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task AssegnaCouponAsync(int couponId, int clienteId)
        {
            var coupon = await _context.Coupons.FindAsync(couponId);
            if (coupon == null || !coupon.Attivo)
                throw new ArgumentException("Coupon non valido o scaduto.");

            if (DateTime.UtcNow > coupon.DataScadenza)
                throw new ArgumentException("Coupon scaduto.");

            var cliente = await _context.Clienti.FindAsync(clienteId);
            if (cliente == null)
                throw new KeyNotFoundException("Cliente non trovato.");

            bool giaAssegnato = await _context.CouponAssegnati
                .AnyAsync(ca => ca.CouponId == couponId && ca.ClienteId == clienteId);

            if (giaAssegnato)
                throw new InvalidOperationException("Coupon già assegnato a questo cliente.");

            var assegnazione = new CouponAssegnato
            {
                CouponId = couponId,
                ClienteId = clienteId,
                DataAssegnazione = DateTime.UtcNow,
                Utilizzato = false,
                Motivo = MotivoAssegnazione.Manuale
            };

            _context.CouponAssegnati.Add(assegnazione);
            await _context.SaveChangesAsync();

            _ = Task.Run(async () => 
            {
                await _emailService.InviaEmailNuovoCouponAsync(
                    cliente.Email, 
                    cliente.Nome, 
                    coupon.Titolo, 
                    coupon.Codice, 
                    coupon.DataScadenza);
            });
        }

        public async Task<List<CouponAssegnatoDTO>> GetCouponsClienteAsync(int clienteId)
        {
             var coupons = await _context.CouponAssegnati
                .Include(ca => ca.Coupon)
                .Where(ca => ca.ClienteId == clienteId)
                .OrderByDescending(ca => ca.DataAssegnazione)
                .ToListAsync();

            return _mapper.Map<List<CouponAssegnatoDTO>>(coupons);
        }

        public async Task RiscattaCouponAsync(int couponAssegnatoId)
        {
            var assegnazione = await _context.CouponAssegnati.FindAsync(couponAssegnatoId);
            
            if (assegnazione == null)
                throw new KeyNotFoundException("Coupon assegnato non trovato.");

            if (assegnazione.Utilizzato)
                throw new InvalidOperationException("Coupon già utilizzato.");

            assegnazione.Utilizzato = true;
            assegnazione.DataUtilizzo = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
