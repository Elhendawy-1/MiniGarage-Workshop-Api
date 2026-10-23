using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MiniGarage.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CarModel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProblemDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RepairStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Cars",
                columns: new[] { "Id", "CarModel", "CreatedAt", "LicensePlate", "OwnerName", "ProblemDescription", "RepairStatus" },
                values: new object[,]
                {
                    { 1, "Toyota Corolla", new DateTime(2026, 1, 10, 8, 0, 0, 0, DateTimeKind.Utc), "ABC-1234", "Ahmed Al-Rashidi", "Engine overheating", 1 },
                    { 2, "Honda Civic", new DateTime(2026, 1, 11, 9, 30, 0, 0, DateTimeKind.Utc), "XYZ-5678", "Sara Al-Mutairi", "Brake pads worn out", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cars_LicensePlate",
                table: "Cars",
                column: "LicensePlate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cars");
        }
    }
}
