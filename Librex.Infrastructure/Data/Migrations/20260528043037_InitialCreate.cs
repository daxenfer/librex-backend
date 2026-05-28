using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Librex.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_return_notes_TenantId_FolioNumber",
                table: "return_notes");

            migrationBuilder.DropIndex(
                name: "IX_remissions_TenantId_FolioNumber",
                table: "remissions");

            migrationBuilder.DropIndex(
                name: "IX_payments_TenantId_FolioNumber",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "return_notes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "return_note_details");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "remissions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "remission_details");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "publishers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "products");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "customers");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDate",
                table: "remissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDueDate",
                table: "remissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnDueDate",
                table: "remissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReturnPercentage",
                table: "remissions",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Teacher",
                table: "remission_details",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "customers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "company_settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BrandName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Rfc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Phone1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Phone2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_return_notes_FolioNumber",
                table: "return_notes",
                column: "FolioNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_remissions_FolioNumber",
                table: "remissions",
                column: "FolioNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_FolioNumber",
                table: "payments",
                column: "FolioNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "company_settings");

            migrationBuilder.DropIndex(
                name: "IX_return_notes_FolioNumber",
                table: "return_notes");

            migrationBuilder.DropIndex(
                name: "IX_remissions_FolioNumber",
                table: "remissions");

            migrationBuilder.DropIndex(
                name: "IX_payments_FolioNumber",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "DeliveryDate",
                table: "remissions");

            migrationBuilder.DropColumn(
                name: "PaymentDueDate",
                table: "remissions");

            migrationBuilder.DropColumn(
                name: "ReturnDueDate",
                table: "remissions");

            migrationBuilder.DropColumn(
                name: "ReturnPercentage",
                table: "remissions");

            migrationBuilder.DropColumn(
                name: "Teacher",
                table: "remission_details");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "City",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "customers");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "return_notes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "return_note_details",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "remissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "remission_details",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "publishers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "customers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_return_notes_TenantId_FolioNumber",
                table: "return_notes",
                columns: new[] { "TenantId", "FolioNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_remissions_TenantId_FolioNumber",
                table: "remissions",
                columns: new[] { "TenantId", "FolioNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_TenantId_FolioNumber",
                table: "payments",
                columns: new[] { "TenantId", "FolioNumber" },
                unique: true);
        }
    }
}
