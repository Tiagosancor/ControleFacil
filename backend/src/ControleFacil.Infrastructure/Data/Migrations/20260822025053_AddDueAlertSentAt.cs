using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFacil.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDueAlertSentAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueAlertSentAt",
                table: "Transactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Status_DueAlertSentAt_EntryDate",
                table: "Transactions",
                columns: new[] { "Status", "DueAlertSentAt", "EntryDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_Status_DueAlertSentAt_EntryDate",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DueAlertSentAt",
                table: "Transactions");
        }
    }
}
