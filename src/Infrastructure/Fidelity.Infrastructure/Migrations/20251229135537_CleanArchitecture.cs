using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fidelity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codice = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Titolo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descrizione = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ValoreSconto = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TipoSconto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Percentuale"),
                    DataInizio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataScadenza = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Attivo = table.Column<bool>(type: "bit", nullable: false),
                    AssegnazioneAutomatica = table.Column<bool>(type: "bit", nullable: false),
                    LimiteUtilizzoGlobale = table.Column<int>(type: "int", nullable: true),
                    LimiteUtilizzoPerCliente = table.Column<int>(type: "int", nullable: true),
                    ImportoMinimoOrdine = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    PuntiRichiesti = table.Column<int>(type: "int", nullable: true),
                    IsCouponBenvenuto = table.Column<bool>(type: "bit", nullable: false),
                    LivelloMinimoRichiesto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UtilizziTotali = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PuntiVendita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codice = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Indirizzo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Citta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CAP = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Provincia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PuntiPerEuro = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0.1m),
                    Attivo = table.Column<bool>(type: "bit", nullable: false),
                    OrariApertura = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuntiVendita", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Responsabili",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NomeCompleto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Ruolo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Responsabile"),
                    Attivo = table.Column<bool>(type: "bit", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    AccountLockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RichiestaResetPassword = table.Column<bool>(type: "bit", nullable: false),
                    UltimoAccesso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UltimoAccessoIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responsabili", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clienti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodiceFidelity = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cognome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DataRegistrazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PuntoVenditaRegistrazioneId = table.Column<int>(type: "int", nullable: true),
                    ResponsabileRegistrazioneId = table.Column<int>(type: "int", nullable: true),
                    PuntiTotali = table.Column<int>(type: "int", nullable: false),
                    PuntiSpesi = table.Column<int>(type: "int", nullable: false),
                    Livello = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Bronze"),
                    Attivo = table.Column<bool>(type: "bit", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    AccountLockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PrivacyAccettata = table.Column<bool>(type: "bit", nullable: false),
                    PrivacyAccettataData = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clienti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clienti_PuntiVendita_PuntoVenditaRegistrazioneId",
                        column: x => x.PuntoVenditaRegistrazioneId,
                        principalTable: "PuntiVendita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clienti_Responsabili_ResponsabileRegistrazioneId",
                        column: x => x.ResponsabileRegistrazioneId,
                        principalTable: "Responsabili",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResponsabilePuntiVendita",
                columns: table => new
                {
                    ResponsabileId = table.Column<int>(type: "int", nullable: false),
                    PuntoVenditaId = table.Column<int>(type: "int", nullable: false),
                    DataAssociazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Principale = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsabilePuntiVendita", x => new { x.ResponsabileId, x.PuntoVenditaId });
                    table.ForeignKey(
                        name: "FK_ResponsabilePuntiVendita_PuntiVendita_PuntoVenditaId",
                        column: x => x.PuntoVenditaId,
                        principalTable: "PuntiVendita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResponsabilePuntiVendita_Responsabili_ResponsabileId",
                        column: x => x.ResponsabileId,
                        principalTable: "Responsabili",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TokenRegistrazione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Token = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    PuntoVenditaId = table.Column<int>(type: "int", nullable: false),
                    ResponsabileId = table.Column<int>(type: "int", nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataScadenza = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Utilizzato = table.Column<bool>(type: "bit", nullable: false),
                    DataUtilizzo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenRegistrazione", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenRegistrazione_PuntiVendita_PuntoVenditaId",
                        column: x => x.PuntoVenditaId,
                        principalTable: "PuntiVendita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TokenRegistrazione_Responsabili_ResponsabileId",
                        column: x => x.ResponsabileId,
                        principalTable: "Responsabili",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CouponAssegnati",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    DataAssegnazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataUtilizzo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Utilizzato = table.Column<bool>(type: "bit", nullable: false),
                    AssegnatoDa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Manuale"),
                    ResponsabileUtilizzoId = table.Column<int>(type: "int", nullable: true),
                    PuntoVenditaUtilizzoId = table.Column<int>(type: "int", nullable: true),
                    TransazioneUtilizzoId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponAssegnati", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponAssegnati_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CouponAssegnati_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CouponAssegnati_PuntiVendita_PuntoVenditaUtilizzoId",
                        column: x => x.PuntoVenditaUtilizzoId,
                        principalTable: "PuntiVendita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CouponAssegnati_Responsabili_ResponsabileUtilizzoId",
                        column: x => x.ResponsabileUtilizzoId,
                        principalTable: "Responsabili",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    JwtId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    RevokedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    RevokedByIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    ResponsabileId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Responsabili_ResponsabileId",
                        column: x => x.ResponsabileId,
                        principalTable: "Responsabili",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transazioni",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    PuntoVenditaId = table.Column<int>(type: "int", nullable: false),
                    ResponsabileId = table.Column<int>(type: "int", nullable: true),
                    DataTransazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImportoSpesa = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PuntiAssegnati = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CouponAssegnatoId = table.Column<int>(type: "int", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Accumulo"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transazioni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transazioni_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transazioni_CouponAssegnati_CouponAssegnatoId",
                        column: x => x.CouponAssegnatoId,
                        principalTable: "CouponAssegnati",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transazioni_PuntiVendita_PuntoVenditaId",
                        column: x => x.PuntoVenditaId,
                        principalTable: "PuntiVendita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transazioni_Responsabili_ResponsabileId",
                        column: x => x.ResponsabileId,
                        principalTable: "Responsabili",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_Attivo_IsDeleted",
                table: "Clienti",
                columns: new[] { "Attivo", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_CodiceFidelity",
                table: "Clienti",
                column: "CodiceFidelity",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_Cognome",
                table: "Clienti",
                column: "Cognome");

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_Email",
                table: "Clienti",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_Nome",
                table: "Clienti",
                column: "Nome");

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_PuntoVenditaRegistrazioneId",
                table: "Clienti",
                column: "PuntoVenditaRegistrazioneId");

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_ResponsabileRegistrazioneId",
                table: "Clienti",
                column: "ResponsabileRegistrazioneId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponAssegnati_ClienteId_Utilizzato",
                table: "CouponAssegnati",
                columns: new[] { "ClienteId", "Utilizzato" });

            migrationBuilder.CreateIndex(
                name: "IX_CouponAssegnati_CouponId_ClienteId",
                table: "CouponAssegnati",
                columns: new[] { "CouponId", "ClienteId" });

            migrationBuilder.CreateIndex(
                name: "IX_CouponAssegnati_PuntoVenditaUtilizzoId",
                table: "CouponAssegnati",
                column: "PuntoVenditaUtilizzoId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponAssegnati_ResponsabileUtilizzoId",
                table: "CouponAssegnati",
                column: "ResponsabileUtilizzoId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_Attivo_IsDeleted",
                table: "Coupons",
                columns: new[] { "Attivo", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_Codice",
                table: "Coupons",
                column: "Codice",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_DataScadenza",
                table: "Coupons",
                column: "DataScadenza");

            migrationBuilder.CreateIndex(
                name: "IX_PuntiVendita_Codice",
                table: "PuntiVendita",
                column: "Codice",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ClienteId",
                table: "RefreshTokens",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiryDate",
                table: "RefreshTokens",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ResponsabileId",
                table: "RefreshTokens",
                column: "ResponsabileId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResponsabilePuntiVendita_PuntoVenditaId",
                table: "ResponsabilePuntiVendita",
                column: "PuntoVenditaId");

            migrationBuilder.CreateIndex(
                name: "IX_Responsabili_Username",
                table: "Responsabili",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenRegistrazione_Email",
                table: "TokenRegistrazione",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_TokenRegistrazione_Email_Utilizzato",
                table: "TokenRegistrazione",
                columns: new[] { "Email", "Utilizzato" });

            migrationBuilder.CreateIndex(
                name: "IX_TokenRegistrazione_PuntoVenditaId",
                table: "TokenRegistrazione",
                column: "PuntoVenditaId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenRegistrazione_ResponsabileId",
                table: "TokenRegistrazione",
                column: "ResponsabileId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenRegistrazione_Token",
                table: "TokenRegistrazione",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transazioni_ClienteId_DataTransazione",
                table: "Transazioni",
                columns: new[] { "ClienteId", "DataTransazione" });

            migrationBuilder.CreateIndex(
                name: "IX_Transazioni_CouponAssegnatoId",
                table: "Transazioni",
                column: "CouponAssegnatoId",
                unique: true,
                filter: "[CouponAssegnatoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transazioni_DataTransazione",
                table: "Transazioni",
                column: "DataTransazione");

            migrationBuilder.CreateIndex(
                name: "IX_Transazioni_PuntoVenditaId",
                table: "Transazioni",
                column: "PuntoVenditaId");

            migrationBuilder.CreateIndex(
                name: "IX_Transazioni_ResponsabileId",
                table: "Transazioni",
                column: "ResponsabileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "ResponsabilePuntiVendita");

            migrationBuilder.DropTable(
                name: "TokenRegistrazione");

            migrationBuilder.DropTable(
                name: "Transazioni");

            migrationBuilder.DropTable(
                name: "CouponAssegnati");

            migrationBuilder.DropTable(
                name: "Clienti");

            migrationBuilder.DropTable(
                name: "Coupons");

            migrationBuilder.DropTable(
                name: "PuntiVendita");

            migrationBuilder.DropTable(
                name: "Responsabili");
        }
    }
}
