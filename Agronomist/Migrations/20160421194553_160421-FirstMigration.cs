using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace Agronomist.Migrations
{
    public partial class _160421FirstMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "PlacementType",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacementType", x => x.ID);
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
                name: "CropType",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Version = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropType", x => x.ID);
                });
            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
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
                name: "ControlType",
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
                    table.PrimaryKey("PK_ControlType", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ControlType_Subsystem_SubsystemID",
                        column: x => x.SubsystemID,
                        principalTable: "Subsystem",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "ParamAtPlace",
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
                    table.PrimaryKey("PK_ParamAtPlace", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ParamAtPlace_Parameter_ParamID",
                        column: x => x.ParamID,
                        principalTable: "Parameter",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParamAtPlace_PlacementType_PlaceID",
                        column: x => x.PlaceID,
                        principalTable: "PlacementType",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParamAtPlace_Subsystem_SubsystemID",
                        column: x => x.SubsystemID,
                        principalTable: "Subsystem",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "Greenhouse",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    PersonId = table.Column<long>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Version = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Greenhouse", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Greenhouse_Person_PersonId",
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
                    GreenhouseID = table.Column<Guid>(nullable: false),
                    Location = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    SerialNumber = table.Column<Guid>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Version = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Device", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Device_Greenhouse_GreenhouseID",
                        column: x => x.GreenhouseID,
                        principalTable: "Greenhouse",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "Controllable",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    ControlTypeID = table.Column<Guid>(nullable: false),
                    ControlTypeID1 = table.Column<long>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    GreenhouseID = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Version = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Controllable", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Controllable_ControlType_ControlTypeID1",
                        column: x => x.ControlTypeID1,
                        principalTable: "ControlType",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Controllable_Greenhouse_GreenhouseID",
                        column: x => x.GreenhouseID,
                        principalTable: "Greenhouse",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "CropCycle",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    CropTypeID = table.Column<Guid>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    EndDate = table.Column<DateTimeOffset>(nullable: true),
                    GreenhouseID = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    StartDate = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Version = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropCycle", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CropCycle_CropType_CropTypeID",
                        column: x => x.CropTypeID,
                        principalTable: "CropType",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CropCycle_Greenhouse_GreenhouseID",
                        column: x => x.GreenhouseID,
                        principalTable: "Greenhouse",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "Sensor",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    AlertHigh = table.Column<double>(nullable: true),
                    AlertLow = table.Column<double>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    DeviceID = table.Column<Guid>(nullable: false),
                    Multiplier = table.Column<double>(nullable: false),
                    Offset = table.Column<double>(nullable: false),
                    ParamAtPLaceID = table.Column<long>(nullable: false),
                    PlacementTypeID = table.Column<long>(nullable: true),
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
                        name: "FK_Sensor_ParamAtPlace_ParamAtPLaceID",
                        column: x => x.ParamAtPLaceID,
                        principalTable: "ParamAtPlace",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sensor_PlacementType_PlacementTypeID",
                        column: x => x.PlacementTypeID,
                        principalTable: "PlacementType",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "Relay",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    ControlableID = table.Column<Guid>(nullable: true),
                    ControllableID = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    DeviceID = table.Column<Guid>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    OffTime = table.Column<uint>(nullable: false),
                    OnTime = table.Column<uint>(nullable: false),
                    StartDate = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Version = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relay", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Relay_Controllable_ControlableID",
                        column: x => x.ControlableID,
                        principalTable: "Controllable",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Relay_Device_DeviceID",
                        column: x => x.DeviceID,
                        principalTable: "Device",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "ControlHistory",
                columns: table => new
                {
                    ControllableID = table.Column<Guid>(nullable: false),
                    DateTime = table.Column<DateTimeOffset>(nullable: false),
                    Controllable = table.Column<Guid>(nullable: false),
                    DataDay = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlHistory", x => new { x.ControllableID, x.DateTime });
                    table.ForeignKey(
                        name: "FK_ControlHistory_Controllable_ControllableID",
                        column: x => x.ControllableID,
                        principalTable: "Controllable",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "SensorData",
                columns: table => new
                {
                    SensorID = table.Column<Guid>(nullable: false),
                    DateTime = table.Column<DateTimeOffset>(nullable: false),
                    DayData = table.Column<byte[]>(nullable: true),
                    GreenhouseID = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorData", x => new { x.SensorID, x.DateTime });
                    table.ForeignKey(
                        name: "FK_SensorData_Greenhouse_GreenhouseID",
                        column: x => x.GreenhouseID,
                        principalTable: "Greenhouse",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SensorData_Sensor_SensorID",
                        column: x => x.SensorID,
                        principalTable: "Sensor",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Relay");
            migrationBuilder.DropTable("ControlHistory");
            migrationBuilder.DropTable("CropCycle");
            migrationBuilder.DropTable("SensorData");
            migrationBuilder.DropTable("Controllable");
            migrationBuilder.DropTable("CropType");
            migrationBuilder.DropTable("Sensor");
            migrationBuilder.DropTable("ControlType");
            migrationBuilder.DropTable("Device");
            migrationBuilder.DropTable("ParamAtPlace");
            migrationBuilder.DropTable("Greenhouse");
            migrationBuilder.DropTable("Parameter");
            migrationBuilder.DropTable("PlacementType");
            migrationBuilder.DropTable("Subsystem");
            migrationBuilder.DropTable("Person");
        }
    }
}
