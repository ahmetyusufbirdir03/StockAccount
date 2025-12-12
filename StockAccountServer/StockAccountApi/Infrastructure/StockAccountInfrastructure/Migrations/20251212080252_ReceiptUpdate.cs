using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockAccountInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReceiptUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StockId",
                table: "Receipt",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Receipt_StockId",
                table: "Receipt",
                column: "StockId");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipt_Stock_StockId",
                table: "Receipt",
                column: "StockId",
                principalTable: "Stock",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipt_Stock_StockId",
                table: "Receipt");

            migrationBuilder.DropIndex(
                name: "IX_Receipt_StockId",
                table: "Receipt");

            migrationBuilder.DropColumn(
                name: "StockId",
                table: "Receipt");
        }
    }
}
