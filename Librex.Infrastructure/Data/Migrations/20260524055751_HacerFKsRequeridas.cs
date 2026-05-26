using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Librex.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class HacerFKsRequeridas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_publishers_PublisherId",
                table: "products");

            // Assign orphan products to the first available publisher (NULL or 0 from a previous failed migration)
            migrationBuilder.Sql(
                @"UPDATE products SET ""PublisherId"" = (SELECT ""Id"" FROM publishers ORDER BY ""Id"" LIMIT 1)
                  WHERE ""PublisherId"" IS NULL OR ""PublisherId"" = 0");

            // Remove return notes (and their details) with no valid remission
            migrationBuilder.Sql(
                @"DELETE FROM return_note_details
                  WHERE ""ReturnNoteId"" IN (SELECT ""Id"" FROM return_notes WHERE ""RemissionId"" IS NULL OR ""RemissionId"" = 0)");
            migrationBuilder.Sql(
                @"DELETE FROM return_notes WHERE ""RemissionId"" IS NULL OR ""RemissionId"" = 0");

            // Remove payments with no valid remission
            migrationBuilder.Sql(
                @"DELETE FROM payments WHERE ""RemissionId"" IS NULL OR ""RemissionId"" = 0");

            migrationBuilder.AlterColumn<int>(
                name: "RemissionId",
                table: "return_notes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PublisherId",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RemissionId",
                table: "payments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_products_publishers_PublisherId",
                table: "products",
                column: "PublisherId",
                principalTable: "publishers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_publishers_PublisherId",
                table: "products");

            migrationBuilder.AlterColumn<int>(
                name: "RemissionId",
                table: "return_notes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "PublisherId",
                table: "products",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "RemissionId",
                table: "payments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_products_publishers_PublisherId",
                table: "products",
                column: "PublisherId",
                principalTable: "publishers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
