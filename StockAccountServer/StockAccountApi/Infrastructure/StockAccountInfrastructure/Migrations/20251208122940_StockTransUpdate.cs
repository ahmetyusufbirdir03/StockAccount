using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockAccountInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StockTransUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CounterpartyCompanyId",
                table: "StockTrans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTrans_CounterpartyCompanyId",
                table: "StockTrans",
                column: "CounterpartyCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockTrans_Companies_CounterpartyCompanyId",
                table: "StockTrans",
                column: "CounterpartyCompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockTrans_Companies_CounterpartyCompanyId",
                table: "StockTrans");

            migrationBuilder.DropIndex(
                name: "IX_StockTrans_CounterpartyCompanyId",
                table: "StockTrans");

            migrationBuilder.DropColumn(
                name: "CounterpartyCompanyId",
                table: "StockTrans");
        }
    }
}
