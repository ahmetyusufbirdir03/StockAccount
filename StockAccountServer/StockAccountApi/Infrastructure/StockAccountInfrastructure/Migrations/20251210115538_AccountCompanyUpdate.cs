using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockAccountInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AccountCompanyUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AccountCompany",
                table: "AccountCompany");

            migrationBuilder.DropIndex(
                name: "IX_AccountCompany_CompanyId",
                table: "AccountCompany");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AccountCompany");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AccountCompany");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AccountCompany");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AccountCompany");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "AccountCompany");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AccountCompany");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AccountCompany");

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "AccountCompany",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "LinkedAt",
                table: "AccountCompany",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccountCompany",
                table: "AccountCompany",
                columns: new[] { "CompanyId", "AccountId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AccountCompany",
                table: "AccountCompany");

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "AccountCompany");

            migrationBuilder.DropColumn(
                name: "LinkedAt",
                table: "AccountCompany");

            migrationBuilder.AddColumn<int>(
                name: "AccountType",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "Accounts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "AccountCompany",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AccountCompany",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AccountCompany",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AccountCompany",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "AccountCompany",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AccountCompany",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AccountCompany",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccountCompany",
                table: "AccountCompany",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AccountCompany_CompanyId",
                table: "AccountCompany",
                column: "CompanyId");
        }
    }
}
