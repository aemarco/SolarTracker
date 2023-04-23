using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarTracker.Migrations
{
    public partial class SunInfoAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolarInfos",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Latitude = table.Column<float>(type: "REAL", nullable: false),
                    Longitude = table.Column<float>(type: "REAL", nullable: false),
                    Sunrise = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    Sunset = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    Altitude = table.Column<float>(type: "REAL", nullable: false),
                    Azimuth = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolarInfos", x => new { x.Timestamp, x.Latitude, x.Longitude });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolarInfos");
        }
    }
}
