using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fidelity.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCoupons : Migration
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
                    Descrizione = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ValoreSconto = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TipoSconto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataInizio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataScadenza = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Attivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Id);
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
                    Utilizzato = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponAssegnati", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponAssegnati_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponAssegnati_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CouponAssegnati_ClienteId",
                table: "CouponAssegnati",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponAssegnati_CouponId",
                table: "CouponAssegnati",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_Codice",
                table: "Coupons",
                column: "Codice",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CouponAssegnati");

            migrationBuilder.DropTable(
                name: "Coupons");
        }
    }
}
