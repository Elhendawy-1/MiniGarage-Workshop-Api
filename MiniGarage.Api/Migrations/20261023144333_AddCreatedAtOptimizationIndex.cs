using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniGarage.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtOptimizationIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Cars_CreatedAt",
                table: "Cars",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cars_CreatedAt",
                table: "Cars");
        }
    }
}
