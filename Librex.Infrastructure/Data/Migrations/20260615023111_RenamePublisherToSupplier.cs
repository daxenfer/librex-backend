using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Librex.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenamePublisherToSupplier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename de Publisher -> Supplier preservando los datos (no drop/create).
            migrationBuilder.DropForeignKey(
                name: "FK_products_publishers_PublisherId",
                table: "products");

            migrationBuilder.RenameTable(
                name: "publishers",
                newName: "suppliers");

            migrationBuilder.Sql(
                "ALTER TABLE suppliers RENAME CONSTRAINT \"PK_publishers\" TO \"PK_suppliers\";");

            migrationBuilder.RenameColumn(
                name: "PublisherId",
                table: "products",
                newName: "SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_products_PublisherId",
                table: "products",
                newName: "IX_products_SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_products_suppliers_SupplierId",
                table: "products",
                column: "SupplierId",
                principalTable: "suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_suppliers_SupplierId",
                table: "products");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "products",
                newName: "PublisherId");

            migrationBuilder.RenameIndex(
                name: "IX_products_SupplierId",
                table: "products",
                newName: "IX_products_PublisherId");

            migrationBuilder.Sql(
                "ALTER TABLE suppliers RENAME CONSTRAINT \"PK_suppliers\" TO \"PK_publishers\";");

            migrationBuilder.RenameTable(
                name: "suppliers",
                newName: "publishers");

            migrationBuilder.AddForeignKey(
                name: "FK_products_publishers_PublisherId",
                table: "products",
                column: "PublisherId",
                principalTable: "publishers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
