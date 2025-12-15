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
            migrationBuilder.DropForeignKey(
                name: "FK_StockTrans_Companies_CounterpartyCompanyId",
                table: "StockTrans");

            migrationBuilder.RenameColumn(
                name: "CounterpartyCompanyId",
                table: "StockTrans",
                newName: "AccountId");

            migrationBuilder.RenameIndex(
                name: "IX_StockTrans_CounterpartyCompanyId",
                table: "StockTrans",
                newName: "IX_StockTrans_AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockTrans_Accounts_AccountId",
                table: "StockTrans",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockTrans_Accounts_AccountId",
                table: "StockTrans");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "StockTrans",
                newName: "CounterpartyCompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_StockTrans_AccountId",
                table: "StockTrans",
                newName: "IX_StockTrans_CounterpartyCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockTrans_Companies_CounterpartyCompanyId",
                table: "StockTrans",
                column: "CounterpartyCompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
