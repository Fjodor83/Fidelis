using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Fidelity.Domain.Entities;

namespace Fidelity.Infrastructure.Persistence;

/// <summary>
/// Database seeder for initial data
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger? logger = null)
    {
        try
        {
            // Ensure database is created and migrated
            await context.Database.MigrateAsync();

            // Seed PuntiVendita
            if (!await context.PuntiVendita.AnyAsync())
            {
                var puntoVendita = new PuntoVendita
                {
                    Codice = "NE01",
                    Nome = "Sede Centrale Suns",
                    Indirizzo = "Via Roma, 1",
                    Citta = "Milano",
                    CAP = "20100",
                    Provincia = "MI",
                    PuntiPerEuro = 0.1m,
                    Attivo = true
                };

                context.PuntiVendita.Add(puntoVendita);
                await context.SaveChangesAsync();

                logger?.LogInformation("Seeded default PuntoVendita");
            }

            // Seed Admin user
            if (!await context.Responsabili.AnyAsync(r => r.Ruolo == "Admin"))
            {
                var admin = new Responsabile
                {
                    Username = "admin",
                    NomeCompleto = "Amministratore Sistema",
                    Email = "admin@suns.it",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Ruolo = "Admin",
                    Attivo = true
                };

                context.Responsabili.Add(admin);
                await context.SaveChangesAsync();

                // Associate admin with first punto vendita
                var pv = await context.PuntiVendita.FirstAsync();
                context.ResponsabilePuntiVendita.Add(new ResponsabilePuntoVendita
                {
                    ResponsabileId = admin.Id,
                    PuntoVenditaId = pv.Id,
                    Principale = true
                });
                await context.SaveChangesAsync();

                logger?.LogInformation("Seeded Admin user");
            }

            // Seed Welcome Coupon
            if (!await context.Coupons.AnyAsync(c => c.IsCouponBenvenuto))
            {
                var welcomeCoupon = new Coupon
                {
                    Codice = "BENVENUTO10",
                    Titolo = "Sconto di Benvenuto",
                    Descrizione = "10% di sconto sul tuo primo acquisto!",
                    ValoreSconto = 10,
                    TipoSconto = TipoSconto.Percentuale,
                    DataInizio = DateTime.UtcNow,
                    DataScadenza = DateTime.UtcNow.AddYears(1),
                    Attivo = true,
                    AssegnazioneAutomatica = false,
                    IsCouponBenvenuto = true,
                    LimiteUtilizzoPerCliente = 1
                };

                context.Coupons.Add(welcomeCoupon);
                await context.SaveChangesAsync();

                logger?.LogInformation("Seeded Welcome Coupon");
            }

            logger?.LogInformation("✓ Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}
