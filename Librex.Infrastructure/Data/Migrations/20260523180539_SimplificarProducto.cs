using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Librex.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SimplificarProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_publishers_PublisherId",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_PublisherId",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "products");

            migrationBuilder.DropColumn(
                name: "ISBN",
                table: "products");

            migrationBuilder.DropColumn(
                name: "MinStock",
                table: "products");

            migrationBuilder.DropColumn(
                name: "PublisherId",
                table: "products");

            migrationBuilder.DropColumn(
                name: "PurchasePrice",
                table: "products");

            migrationBuilder.DropColumn(
                name: "SalePrice",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "products",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ISBN",
                table: "products",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinStock",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PublisherId",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PurchasePrice",
                table: "products",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SalePrice",
                table: "products",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_products_PublisherId",
                table: "products",
                column: "PublisherId");

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
