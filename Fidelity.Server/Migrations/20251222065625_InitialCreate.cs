using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fidelity.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PuntiVendita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codice = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Indirizzo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Citta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attivo = table.Column<bool>(type: "bit", nullable: false)
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
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomeCompleto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PuntoVenditaId = table.Column<int>(type: "int", nullable: true),
                    Ruolo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Attivo = table.Column<bool>(type: "bit", nullable: false),
                    RichiestaResetPassword = table.Column<bool>(type: "bit", nullable: false),
                    UltimoAccesso = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responsabili", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Responsabili_PuntiVendita_PuntoVenditaId",
                        column: x => x.PuntoVenditaId,
                        principalTable: "PuntiVendita",
                        principalColumn: "Id");
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
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataRegistrazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PuntoVenditaRegistrazioneId = table.Column<int>(type: "int", nullable: false),
                    ResponsabileRegistrazioneId = table.Column<int>(type: "int", nullable: false),
                    PuntiTotali = table.Column<int>(type: "int", nullable: false),
                    Attivo = table.Column<bool>(type: "bit", nullable: false),
                    PrivacyAccettata = table.Column<bool>(type: "bit", nullable: false)
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
                name: "TokenRegistrazione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    PuntoVenditaId = table.Column<int>(type: "int", nullable: false),
                    ResponsabileId = table.Column<int>(type: "int", nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataScadenza = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Utilizzato = table.Column<bool>(type: "bit", nullable: false),
                    DataUtilizzo = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenRegistrazione", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenRegistrazione_PuntiVendita_PuntoVenditaId",
                        column: x => x.PuntoVenditaId,
                        principalTable: "PuntiVendita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TokenRegistrazione_Responsabili_ResponsabileId",
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
                    PuntiAssegnati = table.Column<int>(type: "int", nullable: false),
                    ImportoSpesa = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DataTransazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponsabileId = table.Column<int>(type: "int", nullable: false),
                    TipoTransazione = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transazioni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transazioni_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transazioni_PuntiVendita_PuntoVenditaId",
                        column: x => x.PuntoVenditaId,
                        principalTable: "PuntiVendita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transazioni_Responsabili_ResponsabileId",
                        column: x => x.ResponsabileId,
                        principalTable: "Responsabili",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_CodiceFidelity",
                table: "Clienti",
                column: "CodiceFidelity",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_Email",
                table: "Clienti",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_PuntoVenditaRegistrazioneId",
                table: "Clienti",
                column: "PuntoVenditaRegistrazioneId");

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_ResponsabileRegistrazioneId",
                table: "Clienti",
                column: "ResponsabileRegistrazioneId");

            migrationBuilder.CreateIndex(
                name: "IX_PuntiVendita_Codice",
                table: "PuntiVendita",
                column: "Codice",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Responsabili_PuntoVenditaId",
                table: "Responsabili",
                column: "PuntoVenditaId");

            migrationBuilder.CreateIndex(
                name: "IX_Responsabili_Username",
                table: "Responsabili",
                column: "Username",
                unique: true);

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
                name: "IX_Transazioni_ClienteId",
                table: "Transazioni",
                column: "ClienteId");

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
                name: "TokenRegistrazione");

            migrationBuilder.DropTable(
                name: "Transazioni");

            migrationBuilder.DropTable(
                name: "Clienti");

            migrationBuilder.DropTable(
                name: "Responsabili");

            migrationBuilder.DropTable(
                name: "PuntiVendita");
        }
    }
}
