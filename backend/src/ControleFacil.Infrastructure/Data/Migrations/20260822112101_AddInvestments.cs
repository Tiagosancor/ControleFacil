using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ControleFacil.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvestmentCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestmentCategories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvestmentCategoryId = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestmentEntries_InvestmentCategories_InvestmentCategoryId",
                        column: x => x.InvestmentCategoryId,
                        principalTable: "InvestmentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvestmentEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentCategories_UserId",
                table: "InvestmentCategories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentEntries_InvestmentCategoryId",
                table: "InvestmentEntries",
                column: "InvestmentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentEntries_UserId_InvestmentCategoryId_Year_Month",
                table: "InvestmentEntries",
                columns: new[] { "UserId", "InvestmentCategoryId", "Year", "Month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestmentEntries");

            migrationBuilder.DropTable(
                name: "InvestmentCategories");
        }
    }
}
