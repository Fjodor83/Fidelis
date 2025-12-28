using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fidelity.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountLockout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AccountLockedUntil",
                table: "Responsabili",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                table: "Responsabili",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "AccountLockedUntil",
                table: "Clienti",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                table: "Clienti",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountLockedUntil",
                table: "Responsabili");

            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                table: "Responsabili");

            migrationBuilder.DropColumn(
                name: "AccountLockedUntil",
                table: "Clienti");

            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                table: "Clienti");
        }
    }
}
