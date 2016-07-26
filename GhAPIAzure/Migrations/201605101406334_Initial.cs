namespace GhAPIAzure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RelayHistories",
                c => new
                    {
                        RelayID = c.Guid(nullable: false),
                        TimeStamp = c.DateTimeOffset(nullable: false, precision: 7),
                        LocationID = c.Guid(),
                        RawData = c.Binary(),
                    })
                .PrimaryKey(t => new { t.RelayID, t.TimeStamp })
                .ForeignKey("dbo.Relays", t => t.RelayID, cascadeDelete: true)
                .ForeignKey("dbo.Locations", t => t.LocationID)
                .Index(t => t.RelayID)
                .Index(t => t.LocationID);
            
            CreateTable(
                "dbo.Locations",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        PersonId = c.Guid(nullable: false),
                        CreatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        Deleted = c.Boolean(nullable: false),
                        Version = c.Binary(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.People", t => t.PersonId)
                .Index(t => t.PersonId);
            
            CreateTable(
                "dbo.CropCycles",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                        StartDate = c.DateTimeOffset(nullable: false, precision: 7),
                        EndDate = c.DateTimeOffset(precision: 7),
                        CropTypeName = c.String(nullable: false, maxLength: 245),
                        LocationID = c.Guid(nullable: false),
                        CreatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        Deleted = c.Boolean(nullable: false),
                        Version = c.Binary(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.CropTypes", t => t.CropTypeName, cascadeDelete: true)
                .ForeignKey("dbo.Locations", t => t.LocationID, cascadeDelete: true)
                .Index(t => t.CropTypeName)
                .Index(t => t.LocationID);
            
            CreateTable(
                "dbo.CropTypes",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 245),
                        CreatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        Approved = c.Boolean(nullable: false),
                        CreatedBy = c.Guid(),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "dbo.Devices",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                        SerialNumber = c.Guid(nullable: false),
                        LocationID = c.Guid(nullable: false),
                        CreatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        Deleted = c.Boolean(nullable: false),
                        Version = c.Binary(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Locations", t => t.LocationID, cascadeDelete: true)
                .Index(t => t.LocationID);
            
            CreateTable(
                "dbo.Relays",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                        StartDate = c.DateTimeOffset(nullable: false, precision: 7),
                        Enabled = c.Boolean(nullable: false),
                        RelayTypeID = c.Long(nullable: false),
                        DeviceID = c.Guid(nullable: false),
                        CreatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        Deleted = c.Boolean(nullable: false),
                        Version = c.Binary(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Devices", t => t.DeviceID, cascadeDelete: true)
                .ForeignKey("dbo.RelayTypes", t => t.RelayTypeID, cascadeDelete: true)
                .Index(t => t.RelayTypeID)
                .Index(t => t.DeviceID);
            
            CreateTable(
                "dbo.RelayTypes",
                c => new
                    {
                        ID = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                        Additive = c.Boolean(nullable: false),
                        SubsystemID = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Subsystems", t => t.SubsystemID, cascadeDelete: true)
                .Index(t => t.SubsystemID);
            
            CreateTable(
                "dbo.Subsystems",
                c => new
                    {
                        ID = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.SensorTypes",
                c => new
                    {
                        ID = c.Long(nullable: false, identity: true),
                        SubsystemID = c.Long(nullable: false),
                        PlaceID = c.Long(nullable: false),
                        ParamID = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Parameters", t => t.ParamID, cascadeDelete: true)
                .ForeignKey("dbo.Placements", t => t.PlaceID, cascadeDelete: true)
                .ForeignKey("dbo.Subsystems", t => t.SubsystemID, cascadeDelete: true)
                .Index(t => t.SubsystemID)
                .Index(t => t.PlaceID)
                .Index(t => t.ParamID);
            
            CreateTable(
                "dbo.Parameters",
                c => new
                    {
                        ID = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                        Unit = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Placements",
                c => new
                    {
                        ID = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Sensors",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Multiplier = c.Double(nullable: false),
                        Offset = c.Double(nullable: false),
                        AlertHigh = c.Double(nullable: false),
                        AlertLow = c.Double(nullable: false),
                        Enabled = c.Boolean(nullable: false),
                        SensorTypeID = c.Long(nullable: false),
                        DeviceID = c.Guid(nullable: false),
                        CreatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        Deleted = c.Boolean(nullable: false),
                        Version = c.Binary(),
                        Placement_ID = c.Long(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Devices", t => t.DeviceID, cascadeDelete: true)
                .ForeignKey("dbo.SensorTypes", t => t.SensorTypeID, cascadeDelete: true)
                .ForeignKey("dbo.Placements", t => t.Placement_ID)
                .Index(t => t.SensorTypeID)
                .Index(t => t.DeviceID)
                .Index(t => t.Placement_ID);
            
            CreateTable(
                "dbo.SensorHistories",
                c => new
                    {
                        SensorID = c.Guid(nullable: false),
                        TimeStamp = c.DateTimeOffset(nullable: false, precision: 7),
                        LocationID = c.Guid(),
                        RawData = c.Binary(),
                    })
                .PrimaryKey(t => new { t.SensorID, t.TimeStamp })
                .ForeignKey("dbo.Sensors", t => t.SensorID, cascadeDelete: true)
                .ForeignKey("dbo.Locations", t => t.LocationID)
                .Index(t => t.SensorID)
                .Index(t => t.LocationID);
            
            CreateTable(
                "dbo.People",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        TwitterHandle = c.String(),
                        UserName = c.String(),
                        CreatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SensorHistories", "LocationID", "dbo.Locations");
            DropForeignKey("dbo.RelayHistories", "LocationID", "dbo.Locations");
            DropForeignKey("dbo.Locations", "PersonId", "dbo.People");
            DropForeignKey("dbo.Relays", "RelayTypeID", "dbo.RelayTypes");
            DropForeignKey("dbo.SensorTypes", "SubsystemID", "dbo.Subsystems");
            DropForeignKey("dbo.SensorTypes", "PlaceID", "dbo.Placements");
            DropForeignKey("dbo.Sensors", "Placement_ID", "dbo.Placements");
            DropForeignKey("dbo.Sensors", "SensorTypeID", "dbo.SensorTypes");
            DropForeignKey("dbo.SensorHistories", "SensorID", "dbo.Sensors");
            DropForeignKey("dbo.Sensors", "DeviceID", "dbo.Devices");
            DropForeignKey("dbo.SensorTypes", "ParamID", "dbo.Parameters");
            DropForeignKey("dbo.RelayTypes", "SubsystemID", "dbo.Subsystems");
            DropForeignKey("dbo.RelayHistories", "RelayID", "dbo.Relays");
            DropForeignKey("dbo.Relays", "DeviceID", "dbo.Devices");
            DropForeignKey("dbo.Devices", "LocationID", "dbo.Locations");
            DropForeignKey("dbo.CropCycles", "LocationID", "dbo.Locations");
            DropForeignKey("dbo.CropCycles", "CropTypeName", "dbo.CropTypes");
            DropIndex("dbo.SensorHistories", new[] { "LocationID" });
            DropIndex("dbo.SensorHistories", new[] { "SensorID" });
            DropIndex("dbo.Sensors", new[] { "Placement_ID" });
            DropIndex("dbo.Sensors", new[] { "DeviceID" });
            DropIndex("dbo.Sensors", new[] { "SensorTypeID" });
            DropIndex("dbo.SensorTypes", new[] { "ParamID" });
            DropIndex("dbo.SensorTypes", new[] { "PlaceID" });
            DropIndex("dbo.SensorTypes", new[] { "SubsystemID" });
            DropIndex("dbo.RelayTypes", new[] { "SubsystemID" });
            DropIndex("dbo.Relays", new[] { "DeviceID" });
            DropIndex("dbo.Relays", new[] { "RelayTypeID" });
            DropIndex("dbo.Devices", new[] { "LocationID" });
            DropIndex("dbo.CropCycles", new[] { "LocationID" });
            DropIndex("dbo.CropCycles", new[] { "CropTypeName" });
            DropIndex("dbo.Locations", new[] { "PersonId" });
            DropIndex("dbo.RelayHistories", new[] { "LocationID" });
            DropIndex("dbo.RelayHistories", new[] { "RelayID" });
            DropTable("dbo.People");
            DropTable("dbo.SensorHistories");
            DropTable("dbo.Sensors");
            DropTable("dbo.Placements");
            DropTable("dbo.Parameters");
            DropTable("dbo.SensorTypes");
            DropTable("dbo.Subsystems");
            DropTable("dbo.RelayTypes");
            DropTable("dbo.Relays");
            DropTable("dbo.Devices");
            DropTable("dbo.CropTypes");
            DropTable("dbo.CropCycles");
            DropTable("dbo.Locations");
            DropTable("dbo.RelayHistories");
        }
    }
}
