using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Librex.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PaymentAllocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payment_allocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentId = table.Column<int>(type: "integer", nullable: false),
                    RemissionId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_allocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_allocations_payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payment_allocations_remissions_RemissionId",
                        column: x => x.RemissionId,
                        principalTable: "remissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payment_allocations_PaymentId",
                table: "payment_allocations",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_allocations_RemissionId",
                table: "payment_allocations",
                column: "RemissionId");

            // Migra cada pago existente a una asignación contra su remisión actual
            // (antes de eliminar la columna payments.RemissionId).
            migrationBuilder.Sql(@"
                INSERT INTO payment_allocations (""PaymentId"", ""RemissionId"", ""Amount"", ""CreatedAt"", ""IsActive"")
                SELECT ""Id"", ""RemissionId"", ""Amount"", ""CreatedAt"", ""IsActive"" FROM payments;");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_remissions_RemissionId",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_payments_RemissionId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "RemissionId",
                table: "payments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_allocations");

            migrationBuilder.AddColumn<int>(
                name: "RemissionId",
                table: "payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_payments_RemissionId",
                table: "payments",
                column: "RemissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_payments_remissions_RemissionId",
                table: "payments",
                column: "RemissionId",
                principalTable: "remissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
