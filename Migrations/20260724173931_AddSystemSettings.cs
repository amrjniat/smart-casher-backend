using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = 
                    table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FiscalYearStartMonth = table.Column<int>(type: "int", nullable: true),
                    DateFormat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoicePrefix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoicePadding = table.Column<int>(type: "int", nullable: true),
                    InvoiceFooterMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultPrintCopies = table.Column<int>(type: "int", nullable: true),
                    ShowCompanyLogoOnInvoice = table.Column<bool>(type: "bit", nullable: false),
                    DefaultTaxRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TaxCalculationMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultLowStockAlert = table.Column<int>(type: "int", nullable: true),
                    AutoUpdateStockAfterSale = table.Column<bool>(type: "bit", nullable: false),
                    AutoUpdateStockAfterReturn = table.Column<bool>(type: "bit", nullable: false),
                    EnableLowStockAlerts = table.Column<bool>(type: "bit", nullable: false),
                    AllowNegativeStock = table.Column<bool>(type: "bit", nullable: false),
                    UseBarcodeScannerInPOS = table.Column<bool>(type: "bit", nullable: false),
                    DefaultLanguage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AvailableLanguages = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiscountPrefix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiscountSuffix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiscountPosition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");
        }
    }
}
