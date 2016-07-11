namespace Quickbird.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class Initial : Migration
    {
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("CropCycles");

            migrationBuilder.DropTable("RelayHistory");

            migrationBuilder.DropTable("SensorsHistory");

            migrationBuilder.DropTable("CropTypes");

            migrationBuilder.DropTable("Relays");

            migrationBuilder.DropTable("Sensors");

            migrationBuilder.DropTable("RelayTypes");

            migrationBuilder.DropTable("Devices");

            migrationBuilder.DropTable("SensorTypes");

            migrationBuilder.DropTable("Locations");

            migrationBuilder.DropTable("Parameters");

            migrationBuilder.DropTable("Placements");

            migrationBuilder.DropTable("Subsystems");

            migrationBuilder.DropTable("People");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("CropTypes",
                table =>
                    new
                    {
                        Name = table.Column<string>(maxLength: 245, nullable: false),
                        Approved = table.Column<bool>(nullable: false),
                        CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                        CreatedBy = table.Column<Guid>(nullable: true)
                    },
                constraints: table => { table.PrimaryKey("PK_CropTypes", x => x.Name); });

            migrationBuilder.CreateTable("Parameters",
                table =>
                    new
                    {
                        ID = table.Column<long>(nullable: false).Annotation("Autoincrement", true),
                        Name = table.Column<string>(nullable: true),
                        Unit = table.Column<string>(nullable: true)
                    },
                constraints: table => { table.PrimaryKey("PK_Parameters", x => x.ID); });

            migrationBuilder.CreateTable("Placements",
                table =>
                    new
                    {
                        ID = table.Column<long>(nullable: false).Annotation("Autoincrement", true),
                        Name = table.Column<string>(nullable: true)
                    },
                constraints: table => { table.PrimaryKey("PK_Placements", x => x.ID); });

            migrationBuilder.CreateTable("Subsystems",
                table =>
                    new
                    {
                        ID = table.Column<long>(nullable: false).Annotation("Autoincrement", true),
                        Name = table.Column<string>(nullable: true)
                    },
                constraints: table => { table.PrimaryKey("PK_Subsystems", x => x.ID); });

            migrationBuilder.CreateTable("People",
                table =>
                    new
                    {
                        ID = table.Column<Guid>(nullable: false),
                        CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                        TwitterHandle = table.Column<string>(nullable: true),
                        UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                        UserName = table.Column<string>(nullable: true),
                        twitterID = table.Column<ulong>(nullable: false)
                    },
                constraints: table => { table.PrimaryKey("PK_People", x => x.ID); });

            migrationBuilder.CreateTable("RelayTypes",
                table =>
                    new
                    {
                        ID = table.Column<long>(nullable: false).Annotation("Autoincrement", true),
                        Additive = table.Column<bool>(nullable: false),
                        Name = table.Column<string>(nullable: true),
                        SubsystemID = table.Column<long>(nullable: false)
                    }, constraints: table =>
                {
                    table.PrimaryKey("PK_RelayTypes", x => x.ID);
                    table.ForeignKey("FK_RelayTypes_Subsystems_SubsystemID", x => x.SubsystemID, "Subsystems", "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable("SensorTypes",
                table =>
                    new
                    {
                        ID = table.Column<long>(nullable: false).Annotation("Autoincrement", true),
                        ParamID = table.Column<long>(nullable: false),
                        PlaceID = table.Column<long>(nullable: false),
                        SubsystemID = table.Column<long>(nullable: false)
                    }, constraints: table =>
                {
                    table.PrimaryKey("PK_SensorTypes", x => x.ID);
                    table.ForeignKey("FK_SensorTypes_Parameters_ParamID", x => x.ParamID, "Parameters", "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_SensorTypes_Placements_PlaceID", x => x.PlaceID, "Placements", "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_SensorTypes_Subsystems_SubsystemID", x => x.SubsystemID, "Subsystems", "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable("Locations",
                table =>
                    new
                    {
                        ID = table.Column<Guid>(nullable: false),
                        CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                        Deleted = table.Column<bool>(nullable: false),
                        Name = table.Column<string>(nullable: false),
                        PersonId = table.Column<Guid>(nullable: false),
                        UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                        Version = table.Column<byte[]>(nullable: true)
                    }, constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.ID);
                    table.ForeignKey("FK_Locations_People_PersonId", x => x.PersonId, "People", "ID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable("Devices",
                table =>
                    new
                    {
                        ID = table.Column<Guid>(nullable: false),
                        CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                        Deleted = table.Column<bool>(nullable: false),
                        LocationID = table.Column<Guid>(nullable: false),
                        Name = table.Column<string>(nullable: false),
                        SerialNumber = table.Column<Guid>(nullable: false),
                        UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                        Version = table.Column<byte[]>(nullable: true)
                    }, constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.ID);
                    table.ForeignKey("FK_Devices_Locations_LocationID", x => x.LocationID, "Locations", "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable("CropCycles",
                table =>
                    new
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
                    }, constraints: table =>
                {
                    table.PrimaryKey("PK_CropCycles", x => x.ID);
                    table.ForeignKey("FK_CropCycles_CropTypes_CropTypeName", x => x.CropTypeName, "CropTypes", "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_CropCycles_Locations_LocationID", x => x.LocationID, "Locations", "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable("Relays",
                table =>
                    new
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
                    }, constraints: table =>
                {
                    table.PrimaryKey("PK_Relays", x => x.ID);
                    table.ForeignKey("FK_Relays_Devices_DeviceID", x => x.DeviceID, "Devices", "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_Relays_RelayTypes_RelayTypeID", x => x.RelayTypeID, "RelayTypes", "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable("Sensors",
                table =>
                    new
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
                    }, constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.ID);
                    table.ForeignKey("FK_Sensors_Devices_DeviceID", x => x.DeviceID, "Devices", "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_Sensors_Placements_PlacementID", x => x.PlacementID, "Placements", "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_Sensors_SensorTypes_SensorTypeID", x => x.SensorTypeID, "SensorTypes", "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable("RelayHistory",
                table =>
                    new
                    {
                        RelayID = table.Column<Guid>(nullable: false),
                        TimeStamp = table.Column<DateTimeOffset>(nullable: false),
                        LocationID = table.Column<Guid>(nullable: true),
                        RawData = table.Column<byte[]>(nullable: true)
                    }, constraints: table =>
                {
                    table.PrimaryKey("PK_RelayHistory", x => new {x.RelayID, x.TimeStamp});
                    table.ForeignKey("FK_RelayHistory_Locations_LocationID", x => x.LocationID, "Locations", "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_RelayHistory_Relays_RelayID", x => x.RelayID, "Relays", "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable("SensorsHistory",
                table =>
                    new
                    {
                        SensorID = table.Column<Guid>(nullable: false),
                        TimeStamp = table.Column<DateTimeOffset>(nullable: false),
                        LocationID = table.Column<Guid>(nullable: true),
                        RawData = table.Column<byte[]>(nullable: true),
                        UpdatedAt = table.Column<DateTimeOffset>(nullable: false)
                    }, constraints: table =>
                {
                    table.PrimaryKey("PK_SensorsHistory", x => new {x.SensorID, x.TimeStamp});
                    table.ForeignKey("FK_SensorsHistory_Locations_LocationID", x => x.LocationID, "Locations", "ID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey("FK_SensorsHistory_Sensors_SensorID", x => x.SensorID, "Sensors", "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_Devices_LocationID", "Devices", "LocationID");

            migrationBuilder.CreateIndex("IX_RelayTypes_SubsystemID", "RelayTypes", "SubsystemID");

            migrationBuilder.CreateIndex("IX_SensorTypes_ParamID", "SensorTypes", "ParamID");

            migrationBuilder.CreateIndex("IX_SensorTypes_PlaceID", "SensorTypes", "PlaceID");

            migrationBuilder.CreateIndex("IX_SensorTypes_SubsystemID", "SensorTypes", "SubsystemID");

            migrationBuilder.CreateIndex("IX_Relays_DeviceID", "Relays", "DeviceID");

            migrationBuilder.CreateIndex("IX_Relays_RelayTypeID", "Relays", "RelayTypeID");

            migrationBuilder.CreateIndex("IX_Sensors_DeviceID", "Sensors", "DeviceID");

            migrationBuilder.CreateIndex("IX_Sensors_PlacementID", "Sensors", "PlacementID");

            migrationBuilder.CreateIndex("IX_Sensors_SensorTypeID", "Sensors", "SensorTypeID");

            migrationBuilder.CreateIndex("IX_CropCycles_CropTypeName", "CropCycles", "CropTypeName");

            migrationBuilder.CreateIndex("IX_CropCycles_LocationID", "CropCycles", "LocationID");

            migrationBuilder.CreateIndex("IX_Locations_PersonId", "Locations", "PersonId");

            migrationBuilder.CreateIndex("IX_RelayHistory_LocationID", "RelayHistory", "LocationID");

            migrationBuilder.CreateIndex("IX_RelayHistory_RelayID", "RelayHistory", "RelayID");

            migrationBuilder.CreateIndex("IX_SensorsHistory_LocationID", "SensorsHistory", "LocationID");

            migrationBuilder.CreateIndex("IX_SensorsHistory_SensorID", "SensorsHistory", "SensorID");
        }
    }
}
