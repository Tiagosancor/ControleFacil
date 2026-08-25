using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFacil.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentCategoryTypeAndFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AppliedAmount",
                table: "InvestmentCategories",
                type: "numeric(14,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InterestRate",
                table: "InvestmentCategories",
                type: "numeric(7,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyContribution",
                table: "InvestmentCategories",
                type: "numeric(14,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "InvestmentCategories",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppliedAmount",
                table: "InvestmentCategories");

            migrationBuilder.DropColumn(
                name: "InterestRate",
                table: "InvestmentCategories");

            migrationBuilder.DropColumn(
                name: "MonthlyContribution",
                table: "InvestmentCategories");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "InvestmentCategories");
        }
    }
}
