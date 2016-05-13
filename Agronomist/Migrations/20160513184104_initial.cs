using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace Agronomist.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CropType",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Approved = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    CreatedBy = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropType", x => x.Name);
                });
            migrationBuilder.CreateTable(
                name: "Parameter",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Unit = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parameter", x => x.ID);
                });
            migrationBuilder.CreateTable(
                name: "Placement",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Placement", x => x.ID);
                });
            migrationBuilder.CreateTable(
                name: "Subsystem",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subsystem", x => x.ID);
                });
            migrationBuilder.CreateTable(
                name: "Person",
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
                    table.PrimaryKey("PK_Person", x => x.ID);
                });
            migrationBuilder.CreateTable(
                name: "RelayType",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Additive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    SubsystemID = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelayType", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RelayType_Subsystem_SubsystemID",
                        column: x => x.SubsystemID,
                        principalTable: "Subsystem",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "SensorType",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParamID = table.Column<long>(nullable: false),
                    PlaceID = table.Column<long>(nullable: false),
                    SubsystemID = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorType", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SensorType_Parameter_ParamID",
                        column: x => x.ParamID,
                        principalTable: "Parameter",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SensorType_Placement_PlaceID",
                        column: x => x.PlaceID,
                        principalTable: "Placement",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SensorType_Subsystem_SubsystemID",
                        column: x => x.SubsystemID,
                        principalTable: "Subsystem",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "Location",
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
                    table.PrimaryKey("PK_Location", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Location_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                });
            migrationBuilder.CreateTable(
                name: "Device",
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
                    table.PrimaryKey("PK_Device", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Device_Location_LocationID",
                        column: x => x.LocationID,
                        principalTable: "Location",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "CropCycle",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    CropTypeName = table.Column<string>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    EndDate = table.Column<DateTimeOffset>(nullable: true),
                    LocationID = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    StartDate = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Version = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropCycle", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CropCycle_CropType_CropTypeName",
                        column: x => x.CropTypeName,
                        principalTable: "CropType",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CropCycle_Location_LocationID",
                        column: x => x.LocationID,
                        principalTable: "Location",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "Relay",
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
                    table.PrimaryKey("PK_Relay", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Relay_Device_DeviceID",
                        column: x => x.DeviceID,
                        principalTable: "Device",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Relay_RelayType_RelayTypeID",
                        column: x => x.RelayTypeID,
                        principalTable: "RelayType",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "Sensor",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    AlertHigh = table.Column<double>(nullable: false),
                    AlertLow = table.Column<double>(nullable: false),
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
                    table.PrimaryKey("PK_Sensor", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Sensor_Device_DeviceID",
                        column: x => x.DeviceID,
                        principalTable: "Device",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sensor_Placement_PlacementID",
                        column: x => x.PlacementID,
                        principalTable: "Placement",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sensor_SensorType_SensorTypeID",
                        column: x => x.SensorTypeID,
                        principalTable: "SensorType",
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
                        name: "FK_RelayHistory_Location_LocationID",
                        column: x => x.LocationID,
                        principalTable: "Location",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelayHistory_Relay_RelayID",
                        column: x => x.RelayID,
                        principalTable: "Relay",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "SensorHistory",
                columns: table => new
                {
                    SensorID = table.Column<Guid>(nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(nullable: false),
                    LocationID = table.Column<Guid>(nullable: true),
                    RawData = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorHistory", x => new { x.SensorID, x.TimeStamp });
                    table.ForeignKey(
                        name: "FK_SensorHistory_Location_LocationID",
                        column: x => x.LocationID,
                        principalTable: "Location",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SensorHistory_Sensor_SensorID",
                        column: x => x.SensorID,
                        principalTable: "Sensor",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("CropCycle");
            migrationBuilder.DropTable("RelayHistory");
            migrationBuilder.DropTable("SensorHistory");
            migrationBuilder.DropTable("CropType");
            migrationBuilder.DropTable("Relay");
            migrationBuilder.DropTable("Sensor");
            migrationBuilder.DropTable("RelayType");
            migrationBuilder.DropTable("Device");
            migrationBuilder.DropTable("SensorType");
            migrationBuilder.DropTable("Location");
            migrationBuilder.DropTable("Parameter");
            migrationBuilder.DropTable("Placement");
            migrationBuilder.DropTable("Subsystem");
            migrationBuilder.DropTable("Person");
        }
    }
}
