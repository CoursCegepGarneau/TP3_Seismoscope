using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seismoscope.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSensors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DefaultFrequency",
                table: "Sensors",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MaxFrequency",
                table: "Sensors",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MaxThreshold",
                table: "Sensors",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MinThreshold",
                table: "Sensors",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultFrequency",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "MaxFrequency",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "MaxThreshold",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "MinThreshold",
                table: "Sensors");
        }
    }
}
