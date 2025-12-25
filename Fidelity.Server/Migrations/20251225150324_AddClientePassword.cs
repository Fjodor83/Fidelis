using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fidelity.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddClientePassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Clienti",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Clienti",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiry",
                table: "Clienti",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Clienti");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Clienti");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiry",
                table: "Clienti");
        }
    }
}
