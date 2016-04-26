namespace GhAPIAzure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Controllables",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Name = c.String(),
                        GreenhouseID = c.Guid(nullable: false),
                        ControlTypeID = c.Guid(nullable: false),
                        RelayID = c.Guid(),
                        CreatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        Deleted = c.Boolean(nullable: false),
                        Version = c.Binary(),
                        ControlType_ID = c.Long(),
                        Relay_ID = c.Guid(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Greenhouses", t => t.GreenhouseID, cascadeDelete: true)
                .ForeignKey("dbo.ControlTypes", t => t.ControlType_ID)
                .ForeignKey("dbo.Relays", t => t.Relay_ID)
                .Index(t => t.GreenhouseID)
                .Index(t => t.ControlType_ID)
                .Index(t => t.Relay_ID);
            
            CreateTable(
                "dbo.ControlHistories",
                c => new
                    {
                        ControllableID = c.Guid(nullable: false),
                        DateTime = c.DateTimeOffset(nullable: false, precision: 7),
                        Controllable = c.Guid(nullable: false),
                        DataDay = c.Binary(),
                    })
                .PrimaryKey(t => new { t.ControllableID, t.DateTime })
                .ForeignKey("dbo.Controllables", t => t.ControllableID, cascadeDelete: true)
                .Index(t => t.ControllableID);
            
            CreateTable(
                "dbo.ControlTypes",
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
                "dbo.ParamAtPlaces",
                c => new
                    {
                        ID = c.Long(nullable: false, identity: true),
                        SubsystemID = c.Long(nullable: false),
                        PlaceID = c.Long(nullable: false),
                        ParamID = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Parameters", t => t.ParamID, cascadeDelete: true)
                .ForeignKey("dbo.PlacementTypes", t => t.PlaceID, cascadeDelete: true)
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
                "dbo.PlacementTypes",
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
                        AlertHigh = c.Double(),
                        AlertLow = c.Double(),
                        ParamAtPLaceID = c.Long(nullable: false),
                        DeviceID = c.Guid(nullable: false),
                        CreatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        Deleted = c.Boolean(nullable: false),
                        Version = c.Binary(),
                        PlacementType_ID = c.Long(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Devices", t => t.DeviceID, cascadeDelete: true)
                .ForeignKey("dbo.ParamAtPlaces", t => t.ParamAtPLaceID, cascadeDelete: true)
                .ForeignKey("dbo.PlacementTypes", t => t.PlacementType_ID)
                .Index(t => t.ParamAtPLaceID)
                .Index(t => t.DeviceID)
                .Index(t => t.PlacementType_ID);
            
            CreateTable(
                "dbo.Devices",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Name = c.String(),
                        SerialNumber = c.Guid(nullable: false),
                        Location = c.String(),
                        GreenhouseID = c.Guid(nullable: false),
                        CreatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        Deleted = c.Boolean(nullable: false),
                        Version = c.Binary(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Greenhouses", t => t.GreenhouseID, cascadeDelete: true)
                .Index(t => t.GreenhouseID);
            
            CreateTable(
                "dbo.Greenhouses",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Name = c.String(),
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
                        Name = c.String(maxLength: 245),
                        StartDate = c.DateTimeOffset(nullable: false, precision: 7),
                        EndDate = c.DateTimeOffset(precision: 7),
                        CropTypeID = c.Guid(nullable: false),
                        GreenhouseID = c.Guid(nullable: false),
                        CreatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        Deleted = c.Boolean(nullable: false),
                        Version = c.Binary(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.CropTypes", t => t.Name)
                .ForeignKey("dbo.Greenhouses", t => t.GreenhouseID, cascadeDelete: true)
                .Index(t => t.Name)
                .Index(t => t.GreenhouseID);
            
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
            
            CreateTable(
                "dbo.SensorDatas",
                c => new
                    {
                        SensorID = c.Guid(nullable: false),
                        DateTime = c.DateTimeOffset(nullable: false, precision: 7),
                        GreenhouseID = c.Guid(),
                        DayData = c.Binary(),
                    })
                .PrimaryKey(t => new { t.SensorID, t.DateTime })
                .ForeignKey("dbo.Sensors", t => t.SensorID, cascadeDelete: true)
                .ForeignKey("dbo.Greenhouses", t => t.GreenhouseID)
                .Index(t => t.SensorID)
                .Index(t => t.GreenhouseID);
            
            CreateTable(
                "dbo.Relays",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        StartDate = c.DateTimeOffset(nullable: false, precision: 7),
                        Enabled = c.Boolean(nullable: false),
                        ControllableID = c.Guid(nullable: false),
                        DeviceID = c.Guid(nullable: false),
                        CreatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedAt = c.DateTimeOffset(nullable: false, precision: 7),
                        Deleted = c.Boolean(nullable: false),
                        Version = c.Binary(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Devices", t => t.DeviceID, cascadeDelete: true)
                .Index(t => t.DeviceID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Controllables", "Relay_ID", "dbo.Relays");
            DropForeignKey("dbo.Controllables", "ControlType_ID", "dbo.ControlTypes");
            DropForeignKey("dbo.ParamAtPlaces", "SubsystemID", "dbo.Subsystems");
            DropForeignKey("dbo.ParamAtPlaces", "PlaceID", "dbo.PlacementTypes");
            DropForeignKey("dbo.Sensors", "PlacementType_ID", "dbo.PlacementTypes");
            DropForeignKey("dbo.Sensors", "ParamAtPLaceID", "dbo.ParamAtPlaces");
            DropForeignKey("dbo.Sensors", "DeviceID", "dbo.Devices");
            DropForeignKey("dbo.Relays", "DeviceID", "dbo.Devices");
            DropForeignKey("dbo.SensorDatas", "GreenhouseID", "dbo.Greenhouses");
            DropForeignKey("dbo.SensorDatas", "SensorID", "dbo.Sensors");
            DropForeignKey("dbo.Greenhouses", "PersonId", "dbo.People");
            DropForeignKey("dbo.Devices", "GreenhouseID", "dbo.Greenhouses");
            DropForeignKey("dbo.CropCycles", "GreenhouseID", "dbo.Greenhouses");
            DropForeignKey("dbo.CropCycles", "Name", "dbo.CropTypes");
            DropForeignKey("dbo.Controllables", "GreenhouseID", "dbo.Greenhouses");
            DropForeignKey("dbo.ParamAtPlaces", "ParamID", "dbo.Parameters");
            DropForeignKey("dbo.ControlTypes", "SubsystemID", "dbo.Subsystems");
            DropForeignKey("dbo.ControlHistories", "ControllableID", "dbo.Controllables");
            DropIndex("dbo.Relays", new[] { "DeviceID" });
            DropIndex("dbo.SensorDatas", new[] { "GreenhouseID" });
            DropIndex("dbo.SensorDatas", new[] { "SensorID" });
            DropIndex("dbo.CropCycles", new[] { "GreenhouseID" });
            DropIndex("dbo.CropCycles", new[] { "Name" });
            DropIndex("dbo.Greenhouses", new[] { "PersonId" });
            DropIndex("dbo.Devices", new[] { "GreenhouseID" });
            DropIndex("dbo.Sensors", new[] { "PlacementType_ID" });
            DropIndex("dbo.Sensors", new[] { "DeviceID" });
            DropIndex("dbo.Sensors", new[] { "ParamAtPLaceID" });
            DropIndex("dbo.ParamAtPlaces", new[] { "ParamID" });
            DropIndex("dbo.ParamAtPlaces", new[] { "PlaceID" });
            DropIndex("dbo.ParamAtPlaces", new[] { "SubsystemID" });
            DropIndex("dbo.ControlTypes", new[] { "SubsystemID" });
            DropIndex("dbo.ControlHistories", new[] { "ControllableID" });
            DropIndex("dbo.Controllables", new[] { "Relay_ID" });
            DropIndex("dbo.Controllables", new[] { "ControlType_ID" });
            DropIndex("dbo.Controllables", new[] { "GreenhouseID" });
            DropTable("dbo.Relays");
            DropTable("dbo.SensorDatas");
            DropTable("dbo.People");
            DropTable("dbo.CropTypes");
            DropTable("dbo.CropCycles");
            DropTable("dbo.Greenhouses");
            DropTable("dbo.Devices");
            DropTable("dbo.Sensors");
            DropTable("dbo.PlacementTypes");
            DropTable("dbo.Parameters");
            DropTable("dbo.ParamAtPlaces");
            DropTable("dbo.Subsystems");
            DropTable("dbo.ControlTypes");
            DropTable("dbo.ControlHistories");
            DropTable("dbo.Controllables");
        }
    }
}
