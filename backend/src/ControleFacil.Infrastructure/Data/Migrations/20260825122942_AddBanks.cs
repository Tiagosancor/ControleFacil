using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ControleFacil.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBanks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankIspb",
                table: "BankAccounts",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Banks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ispb = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Code = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banks", x => x.Id);
                    table.UniqueConstraint("AK_Banks_Ispb", x => x.Ispb);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_BankIspb",
                table: "BankAccounts",
                column: "BankIspb");

            migrationBuilder.CreateIndex(
                name: "IX_Banks_Ispb",
                table: "Banks",
                column: "Ispb",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Banks_Name",
                table: "Banks",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_Banks_BankIspb",
                table: "BankAccounts",
                column: "BankIspb",
                principalTable: "Banks",
                principalColumn: "Ispb",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccounts_Banks_BankIspb",
                table: "BankAccounts");

            migrationBuilder.DropTable(
                name: "Banks");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_BankIspb",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "BankIspb",
                table: "BankAccounts");
        }
    }
}
