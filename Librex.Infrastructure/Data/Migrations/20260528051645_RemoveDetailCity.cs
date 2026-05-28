using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Librex.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDetailCity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "remission_details");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "remission_details",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
