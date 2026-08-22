using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ControleFacil.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLongTermGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LongTermGoals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TargetAmount = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    TargetYear = table.Column<int>(type: "integer", nullable: false),
                    TargetMonth = table.Column<int>(type: "integer", nullable: false),
                    ManualCurrentAmount = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    InvestmentCategoryId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LongTermGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LongTermGoals_InvestmentCategories_InvestmentCategoryId",
                        column: x => x.InvestmentCategoryId,
                        principalTable: "InvestmentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LongTermGoals_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LongTermGoals_InvestmentCategoryId",
                table: "LongTermGoals",
                column: "InvestmentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_LongTermGoals_UserId",
                table: "LongTermGoals",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LongTermGoals");
        }
    }
}
