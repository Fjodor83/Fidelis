// Fidelity.Server/Data/DatabaseSeeder.cs
using Fidelity.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Server.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Assicurati che il database sia creato
            await context.Database.EnsureCreatedAsync();

            // Se ci sono già dati, non fare nulla
            if (await context.PuntiVendita.AnyAsync())
            {
                return; // Database già popolato
            }

            // Hash BCrypt per password "Suns2024!"
            const string hashedPassword = "$2a$11$ZL/PCQPBe9NKMOgFHY0EheXQ8IXFCX4X7awlP2uAYdLwZA1hYGyJa";

            // Seed Punti Vendita
            var puntiVendita = new[]
            {
                new PuntoVendita
                {
                    Codice = "NE01",
                    Nome = "Suns Centro",
                    Citta = "Roma",
                    Indirizzo = "Via del Corso 1",
                    Telefono = "+39 06 1234567",
                    Attivo = true
                },
                new PuntoVendita
                {
                    Codice = "NE02",
                    Nome = "Suns Nord",
                    Citta = "Milano",
                    Indirizzo = "Corso Vittorio Emanuele II 1",
                    Telefono = "+39 02 7654321",
                    Attivo = true
                }
            };

            await context.PuntiVendita.AddRangeAsync(puntiVendita);
            await context.SaveChangesAsync();

            // Recupera i PuntiVendita appena creati per avere gli ID
            var puntoVendita1 = await context.PuntiVendita.FirstAsync(p => p.Codice == "NE01");
            var puntoVendita2 = await context.PuntiVendita.FirstAsync(p => p.Codice == "NE02");

            // Seed Responsabili
            var responsabili = new[]
            {
                new Responsabile
                {
                    Username = "RE01",
                    PasswordHash = hashedPassword,
                    NomeCompleto = "Mario Rossi",
                    Email = "mario.rossi@sunscompany.com",
                    PuntoVenditaId = puntoVendita1.Id,
                    Ruolo = "Responsabile",
                    Attivo = true,
                    RichiestaResetPassword = false
                },
                new Responsabile
                {
                    Username = "RE02",
                    PasswordHash = hashedPassword,
                    NomeCompleto = "Laura Bianchi",
                    Email = "laura.bianchi@sunscompany.com",
                    PuntoVenditaId = puntoVendita2.Id,
                    Ruolo = "Responsabile",
                    Attivo = true,
                    RichiestaResetPassword = false
                },
                new Responsabile
                {
                    Username = "admin",
                    PasswordHash = hashedPassword,
                    NomeCompleto = "Amministratore Suns",
                    Email = "admin@sunscompany.com",
                    PuntoVenditaId = null,
                    Ruolo = "Admin",
                    Attivo = true,
                    RichiestaResetPassword = false
                }
            };

            await context.Responsabili.AddRangeAsync(responsabili);
            await context.SaveChangesAsync();

            Console.WriteLine("✓ Database seeded successfully!");
        }
    }
}