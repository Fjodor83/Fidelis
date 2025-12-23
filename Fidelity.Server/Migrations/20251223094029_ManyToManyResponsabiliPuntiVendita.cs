using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fidelity.Server.Migrations
{
    /// <inheritdoc />
    public partial class ManyToManyResponsabiliPuntiVendita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Responsabili_PuntiVendita_PuntoVenditaId",
                table: "Responsabili");

            migrationBuilder.DropIndex(
                name: "IX_Responsabili_PuntoVenditaId",
                table: "Responsabili");

            migrationBuilder.DropColumn(
                name: "PuntoVenditaId",
                table: "Responsabili");

            migrationBuilder.CreateTable(
                name: "ResponsabilePuntiVendita",
                columns: table => new
                {
                    ResponsabileId = table.Column<int>(type: "int", nullable: false),
                    PuntoVenditaId = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_ResponsabilePuntiVendita_PuntoVenditaId",
                table: "ResponsabilePuntiVendita",
                column: "PuntoVenditaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResponsabilePuntiVendita");

            migrationBuilder.AddColumn<int>(
                name: "PuntoVenditaId",
                table: "Responsabili",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Responsabili_PuntoVenditaId",
                table: "Responsabili",
                column: "PuntoVenditaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Responsabili_PuntiVendita_PuntoVenditaId",
                table: "Responsabili",
                column: "PuntoVenditaId",
                principalTable: "PuntiVendita",
                principalColumn: "Id");
        }
    }
}
