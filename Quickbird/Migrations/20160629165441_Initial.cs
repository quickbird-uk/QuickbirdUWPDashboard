using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Quickbird.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CropTypes",
                columns: table => new
                {
                    Name = table.Column<string>(maxLength: 245, nullable: false),
                    Approved = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    CreatedBy = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropTypes", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Parameters",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Unit = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parameters", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Placements",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Autoincrement", true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Placements", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Subsystems",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Autoincrement", true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subsystems", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    TwitterHandle = table.Column<string>(nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UserName = table.Column<string>(nullable: true),
                    twitterID = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "RelayTypes",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Autoincrement", true),
                    Additive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    SubsystemID = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelayTypes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RelayTypes_Subsystems_SubsystemID",
                        column: x => x.SubsystemID,
                        principalTable: "Subsystems",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SensorTypes",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Autoincrement", true),
                    ParamID = table.Column<long>(nullable: false),
                    PlaceID = table.Column<long>(nullable: false),
                    SubsystemID = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorTypes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SensorTypes_Parameters_ParamID",
                        column: x => x.ParamID,
                        principalTable: "Parameters",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SensorTypes_Placements_PlaceID",
                        column: x => x.PlaceID,
                        principalTable: "Placements",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SensorTypes_Subsystems_SubsystemID",
                        column: x => x.SubsystemID,
                        principalTable: "Subsystems",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    PersonId = table.Column<Guid>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Version = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Locations_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    LocationID = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    SerialNumber = table.Column<Guid>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Version = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Devices_Locations_LocationID",
                        column: x => x.LocationID,
                        principalTable: "Locations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CropCycles",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    CropTypeName = table.Column<string>(nullable: false),
                    CropVariety = table.Column<string>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    EndDate = table.Column<DateTimeOffset>(nullable: true),
                    LocationID = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    StartDate = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Version = table.Column<byte[]>(nullable: true),
                    Yield = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropCycles", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CropCycles_CropTypes_CropTypeName",
                        column: x => x.CropTypeName,
                        principalTable: "CropTypes",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CropCycles_Locations_LocationID",
                        column: x => x.LocationID,
                        principalTable: "Locations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Relays",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    DeviceID = table.Column<Guid>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    OffTime = table.Column<int>(nullable: false),
                    OnTime = table.Column<int>(nullable: false),
                    RelayTypeID = table.Column<long>(nullable: false),
                    StartDate = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Version = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relays", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Relays_Devices_DeviceID",
                        column: x => x.DeviceID,
                        principalTable: "Devices",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Relays_RelayTypes_RelayTypeID",
                        column: x => x.RelayTypeID,
                        principalTable: "RelayTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Alarmed = table.Column<bool>(nullable: false),
                    AlertHigh = table.Column<double>(nullable: true),
                    AlertLow = table.Column<double>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    DeviceID = table.Column<Guid>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    Multiplier = table.Column<double>(nullable: false),
                    Offset = table.Column<double>(nullable: false),
                    PlacementID = table.Column<long>(nullable: true),
                    SensorTypeID = table.Column<long>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Version = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Sensors_Devices_DeviceID",
                        column: x => x.DeviceID,
                        principalTable: "Devices",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sensors_Placements_PlacementID",
                        column: x => x.PlacementID,
                        principalTable: "Placements",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sensors_SensorTypes_SensorTypeID",
                        column: x => x.SensorTypeID,
                        principalTable: "SensorTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelayHistory",
                columns: table => new
                {
                    RelayID = table.Column<Guid>(nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(nullable: false),
                    LocationID = table.Column<Guid>(nullable: true),
                    RawData = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelayHistory", x => new { x.RelayID, x.TimeStamp });
                    table.ForeignKey(
                        name: "FK_RelayHistory_Locations_LocationID",
                        column: x => x.LocationID,
                        principalTable: "Locations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelayHistory_Relays_RelayID",
                        column: x => x.RelayID,
                        principalTable: "Relays",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SensorsHistory",
                columns: table => new
                {
                    SensorID = table.Column<Guid>(nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(nullable: false),
                    LocationID = table.Column<Guid>(nullable: true),
                    RawData = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorsHistory", x => new { x.SensorID, x.TimeStamp });
                    table.ForeignKey(
                        name: "FK_SensorsHistory_Locations_LocationID",
                        column: x => x.LocationID,
                        principalTable: "Locations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SensorsHistory_Sensors_SensorID",
                        column: x => x.SensorID,
                        principalTable: "Sensors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_LocationID",
                table: "Devices",
                column: "LocationID");

            migrationBuilder.CreateIndex(
                name: "IX_RelayTypes_SubsystemID",
                table: "RelayTypes",
                column: "SubsystemID");

            migrationBuilder.CreateIndex(
                name: "IX_SensorTypes_ParamID",
                table: "SensorTypes",
                column: "ParamID");

            migrationBuilder.CreateIndex(
                name: "IX_SensorTypes_PlaceID",
                table: "SensorTypes",
                column: "PlaceID");

            migrationBuilder.CreateIndex(
                name: "IX_SensorTypes_SubsystemID",
                table: "SensorTypes",
                column: "SubsystemID");

            migrationBuilder.CreateIndex(
                name: "IX_Relays_DeviceID",
                table: "Relays",
                column: "DeviceID");

            migrationBuilder.CreateIndex(
                name: "IX_Relays_RelayTypeID",
                table: "Relays",
                column: "RelayTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_DeviceID",
                table: "Sensors",
                column: "DeviceID");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_PlacementID",
                table: "Sensors",
                column: "PlacementID");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_SensorTypeID",
                table: "Sensors",
                column: "SensorTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_CropCycles_CropTypeName",
                table: "CropCycles",
                column: "CropTypeName");

            migrationBuilder.CreateIndex(
                name: "IX_CropCycles_LocationID",
                table: "CropCycles",
                column: "LocationID");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_PersonId",
                table: "Locations",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_RelayHistory_LocationID",
                table: "RelayHistory",
                column: "LocationID");

            migrationBuilder.CreateIndex(
                name: "IX_RelayHistory_RelayID",
                table: "RelayHistory",
                column: "RelayID");

            migrationBuilder.CreateIndex(
                name: "IX_SensorsHistory_LocationID",
                table: "SensorsHistory",
                column: "LocationID");

            migrationBuilder.CreateIndex(
                name: "IX_SensorsHistory_SensorID",
                table: "SensorsHistory",
                column: "SensorID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CropCycles");

            migrationBuilder.DropTable(
                name: "RelayHistory");

            migrationBuilder.DropTable(
                name: "SensorsHistory");

            migrationBuilder.DropTable(
                name: "CropTypes");

            migrationBuilder.DropTable(
                name: "Relays");

            migrationBuilder.DropTable(
                name: "Sensors");

            migrationBuilder.DropTable(
                name: "RelayTypes");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "SensorTypes");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Parameters");

            migrationBuilder.DropTable(
                name: "Placements");

            migrationBuilder.DropTable(
                name: "Subsystems");

            migrationBuilder.DropTable(
                name: "People");
        }
    }
}
