using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Librex.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPublisherAProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PublisherId",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "remissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FolioNumber = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SalesPerson = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    RecipientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Discount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_remissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_remissions_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "remission_details",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RemissionId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_remission_details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_remission_details_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_remission_details_remissions_RemissionId",
                        column: x => x.RemissionId,
                        principalTable: "remissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_products_PublisherId",
                table: "products",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_remission_details_ProductId",
                table: "remission_details",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_remission_details_RemissionId",
                table: "remission_details",
                column: "RemissionId");

            migrationBuilder.CreateIndex(
                name: "IX_remissions_CustomerId",
                table: "remissions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_remissions_TenantId_FolioNumber",
                table: "remissions",
                columns: new[] { "TenantId", "FolioNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_products_publishers_PublisherId",
                table: "products",
                column: "PublisherId",
                principalTable: "publishers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_publishers_PublisherId",
                table: "products");

            migrationBuilder.DropTable(
                name: "remission_details");

            migrationBuilder.DropTable(
                name: "remissions");

            migrationBuilder.DropIndex(
                name: "IX_products_PublisherId",
                table: "products");

            migrationBuilder.DropColumn(
                name: "PublisherId",
                table: "products");
        }
    }
}
