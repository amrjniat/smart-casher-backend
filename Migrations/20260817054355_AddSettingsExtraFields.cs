using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingsExtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoPrintOnPayment",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CurrencySymbolPosition",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DecimalPlaces",
                table: "SystemSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultDirection",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RoundDecimals",
                table: "SystemSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SystemName",
                table: "SystemSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BranchName",
                table: "CompanyInfos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoPrintOnPayment",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "CurrencySymbolPosition",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "DecimalPlaces",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "DefaultDirection",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "RoundDecimals",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SystemName",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "BranchName",
                table: "CompanyInfos");
        }
    }
}
