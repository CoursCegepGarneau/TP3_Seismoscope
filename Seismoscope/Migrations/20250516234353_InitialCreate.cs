﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seismoscope.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", nullable: false),
                    Région = table.Column<string>(type: "TEXT", nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Frequency = table.Column<double>(type: "REAL", nullable: false),
                    DefaultFrequency = table.Column<double>(type: "REAL", nullable: false),
                    MaxFrequency = table.Column<double>(type: "REAL", nullable: false),
                    Treshold = table.Column<double>(type: "REAL", nullable: false),
                    MinThreshold = table.Column<double>(type: "REAL", nullable: false),
                    MaxThreshold = table.Column<double>(type: "REAL", nullable: false),
                    Delivered = table.Column<bool>(type: "INTEGER", nullable: false),
                    Operational = table.Column<bool>(type: "INTEGER", nullable: false),
                    SensorStatus = table.Column<bool>(type: "INTEGER", nullable: false),
                    Usage = table.Column<int>(type: "INTEGER", nullable: false),
                    assignedStationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sensors_Stations_assignedStationId",
                        column: x => x.assignedStationId,
                        principalTable: "Stations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Prenom = table.Column<string>(type: "TEXT", nullable: true),
                    Nom = table.Column<string>(type: "TEXT", nullable: true),
                    Username = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    Discriminator = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    StationId1 = table.Column<int>(type: "INTEGER", nullable: true),
                    StationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_Stations_StationId1",
                        column: x => x.StationId1,
                        principalTable: "Stations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Historiques",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateHeure = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TypeOnde = table.Column<string>(type: "TEXT", nullable: false),
                    Amplitude = table.Column<double>(type: "REAL", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: false),
                    SeuilAuMoment = table.Column<double>(type: "REAL", nullable: false),
                    SensorId = table.Column<int>(type: "INTEGER", nullable: false),
                    SensorName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historiques", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Historiques_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Historiques_SensorId",
                table: "Historiques",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_assignedStationId",
                table: "Sensors",
                column: "assignedStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_StationId",
                table: "Users",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_StationId1",
                table: "Users",
                column: "StationId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Historiques");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Sensors");

            migrationBuilder.DropTable(
                name: "Stations");
        }
    }
}
