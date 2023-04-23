using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarTracker.Migrations
{
    public partial class SunInfoAddedSecondsOfDay : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "SecondsOfDay",
                table: "SolarInfos",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecondsOfDay",
                table: "SolarInfos");
        }
    }
}
