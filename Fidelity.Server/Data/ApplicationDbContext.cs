// Fidelity.Server/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using Fidelity.Shared.Models;

namespace Fidelity.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clienti { get; set; }
        public DbSet<PuntoVendita> PuntiVendita { get; set; }
        public DbSet<Responsabile> Responsabili { get; set; }
        public DbSet<TokenRegistrazione> TokenRegistrazione { get; set; }
        public DbSet<Transazione> Transazioni { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Indici unici
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.CodiceFidelity)
                .IsUnique();

            modelBuilder.Entity<PuntoVendita>()
                .HasIndex(p => p.Codice)
                .IsUnique();

            modelBuilder.Entity<Responsabile>()
                .HasIndex(r => r.Username)
                .IsUnique();

            modelBuilder.Entity<TokenRegistrazione>()
                .HasIndex(t => t.Token)
                .IsUnique();

            // Relazioni
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.PuntoVenditaRegistrazione)
                .WithMany(p => p.ClientiRegistrati)
                .HasForeignKey(c => c.PuntoVenditaRegistrazioneId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.ResponsabileRegistrazione)
                .WithMany()
                .HasForeignKey(c => c.ResponsabileRegistrazioneId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed dati iniziali - Punti vendita Suns - Zero&Company
            modelBuilder.Entity<PuntoVendita>().HasData(
                new PuntoVendita
                {
                    Id = 1,
                    Codice = "SU01",
                    Nome = "Suns Centro",
                    Citta = "Roma",
                    Indirizzo = "Via del Corso 1",
                    Attivo = true
                },
                new PuntoVendita
                {
                    Id = 2,
                    Codice = "SU02",
                    Nome = "Suns Nord",
                    Citta = "Milano",
                    Indirizzo = "Corso Vittorio Emanuele II 1",
                    Attivo = true
                }
            );

            // Seed responsabili con password hashate (BCrypt)
            // Password per tutti: "Suns2024!"
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Suns2024!");

            modelBuilder.Entity<Responsabile>().HasData(
                new Responsabile
                {
                    Id = 1,
                    Username = "SU01",
                    PasswordHash = hashedPassword,
                    NomeCompleto = "Mario Rossi",
                    PuntoVenditaId = 1,
                    Ruolo = "Responsabile",
                    Attivo = true
                },
                new Responsabile
                {
                    Id = 2,
                    Username = "SU02",
                    PasswordHash = hashedPassword,
                    NomeCompleto = "Laura Bianchi",
                    PuntoVenditaId = 2,
                    Ruolo = "Responsabile",
                    Attivo = true
                },
                new Responsabile
                {
                    Id = 3,
                    Username = "admin",
                    PasswordHash = hashedPassword,
                    NomeCompleto = "Amministratore Suns",
                    PuntoVenditaId = null,
                    Ruolo = "Admin",
                    Attivo = true
                }
            );
        }
    }
}