namespace GhAPIAzure.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Initial : DbMigration
    {
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
            DropIndex("dbo.SensorHistories", new[] {"LocationID"});
            DropIndex("dbo.SensorHistories", new[] {"SensorID"});
            DropIndex("dbo.Sensors", new[] {"Placement_ID"});
            DropIndex("dbo.Sensors", new[] {"DeviceID"});
            DropIndex("dbo.Sensors", new[] {"SensorTypeID"});
            DropIndex("dbo.SensorTypes", new[] {"ParamID"});
            DropIndex("dbo.SensorTypes", new[] {"PlaceID"});
            DropIndex("dbo.SensorTypes", new[] {"SubsystemID"});
            DropIndex("dbo.RelayTypes", new[] {"SubsystemID"});
            DropIndex("dbo.Relays", new[] {"DeviceID"});
            DropIndex("dbo.Relays", new[] {"RelayTypeID"});
            DropIndex("dbo.Devices", new[] {"LocationID"});
            DropIndex("dbo.CropCycles", new[] {"LocationID"});
            DropIndex("dbo.CropCycles", new[] {"CropTypeName"});
            DropIndex("dbo.Locations", new[] {"PersonId"});
            DropIndex("dbo.RelayHistories", new[] {"LocationID"});
            DropIndex("dbo.RelayHistories", new[] {"RelayID"});
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

        public override void Up()
        {
            CreateTable("dbo.RelayHistories",
                    c =>
                        new
                        {
                            RelayID = c.Guid(false),
                            TimeStamp = c.DateTimeOffset(false, 7),
                            LocationID = c.Guid(),
                            RawData = c.Binary()
                        })
                .PrimaryKey(t => new {t.RelayID, t.TimeStamp})
                .ForeignKey("dbo.Relays", t => t.RelayID, true)
                .ForeignKey("dbo.Locations", t => t.LocationID)
                .Index(t => t.RelayID)
                .Index(t => t.LocationID);

            CreateTable("dbo.Locations",
                    c =>
                        new
                        {
                            ID = c.Guid(false),
                            PersonId = c.Guid(false),
                            CreatedAt = c.DateTimeOffset(false, 7),
                            UpdatedAt = c.DateTimeOffset(false, 7),
                            Deleted = c.Boolean(false),
                            Version = c.Binary()
                        })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.People", t => t.PersonId)
                .Index(t => t.PersonId);

            CreateTable("dbo.CropCycles",
                    c =>
                        new
                        {
                            ID = c.Guid(false),
                            Name = c.String(false),
                            StartDate = c.DateTimeOffset(false, 7),
                            EndDate = c.DateTimeOffset(precision: 7),
                            CropTypeName = c.String(false, 245),
                            LocationID = c.Guid(false),
                            CreatedAt = c.DateTimeOffset(false, 7),
                            UpdatedAt = c.DateTimeOffset(false, 7),
                            Deleted = c.Boolean(false),
                            Version = c.Binary()
                        })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.CropTypes", t => t.CropTypeName, true)
                .ForeignKey("dbo.Locations", t => t.LocationID, true)
                .Index(t => t.CropTypeName)
                .Index(t => t.LocationID);

            CreateTable("dbo.CropTypes",
                c =>
                    new
                    {
                        Name = c.String(false, 245),
                        CreatedAt = c.DateTimeOffset(false, 7),
                        Approved = c.Boolean(false),
                        CreatedBy = c.Guid()
                    }).PrimaryKey(t => t.Name);

            CreateTable("dbo.Devices",
                    c =>
                        new
                        {
                            ID = c.Guid(false),
                            Name = c.String(false),
                            SerialNumber = c.Guid(false),
                            LocationID = c.Guid(false),
                            CreatedAt = c.DateTimeOffset(false, 7),
                            UpdatedAt = c.DateTimeOffset(false, 7),
                            Deleted = c.Boolean(false),
                            Version = c.Binary()
                        })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Locations", t => t.LocationID, true)
                .Index(t => t.LocationID);

            CreateTable("dbo.Relays",
                    c =>
                        new
                        {
                            ID = c.Guid(false),
                            Name = c.String(false),
                            StartDate = c.DateTimeOffset(false, 7),
                            Enabled = c.Boolean(false),
                            RelayTypeID = c.Long(false),
                            DeviceID = c.Guid(false),
                            CreatedAt = c.DateTimeOffset(false, 7),
                            UpdatedAt = c.DateTimeOffset(false, 7),
                            Deleted = c.Boolean(false),
                            Version = c.Binary()
                        })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Devices", t => t.DeviceID, true)
                .ForeignKey("dbo.RelayTypes", t => t.RelayTypeID, true)
                .Index(t => t.RelayTypeID)
                .Index(t => t.DeviceID);

            CreateTable("dbo.RelayTypes",
                    c =>
                        new
                        {
                            ID = c.Long(false, true),
                            Name = c.String(),
                            Additive = c.Boolean(false),
                            SubsystemID = c.Long(false)
                        })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Subsystems", t => t.SubsystemID, true)
                .Index(t => t.SubsystemID);

            CreateTable("dbo.Subsystems", c => new {ID = c.Long(false, true), Name = c.String()}).PrimaryKey(t => t.ID);

            CreateTable("dbo.SensorTypes",
                    c =>
                        new
                        {
                            ID = c.Long(false, true),
                            SubsystemID = c.Long(false),
                            PlaceID = c.Long(false),
                            ParamID = c.Long(false)
                        })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Parameters", t => t.ParamID, true)
                .ForeignKey("dbo.Placements", t => t.PlaceID, true)
                .ForeignKey("dbo.Subsystems", t => t.SubsystemID, true)
                .Index(t => t.SubsystemID)
                .Index(t => t.PlaceID)
                .Index(t => t.ParamID);

            CreateTable("dbo.Parameters", c => new {ID = c.Long(false, true), Name = c.String(), Unit = c.String()})
                .PrimaryKey(t => t.ID);

            CreateTable("dbo.Placements", c => new {ID = c.Long(false, true), Name = c.String()}).PrimaryKey(t => t.ID);

            CreateTable("dbo.Sensors",
                    c =>
                        new
                        {
                            ID = c.Guid(false),
                            Multiplier = c.Double(false),
                            Offset = c.Double(false),
                            AlertHigh = c.Double(false),
                            AlertLow = c.Double(false),
                            Enabled = c.Boolean(false),
                            SensorTypeID = c.Long(false),
                            DeviceID = c.Guid(false),
                            CreatedAt = c.DateTimeOffset(false, 7),
                            UpdatedAt = c.DateTimeOffset(false, 7),
                            Deleted = c.Boolean(false),
                            Version = c.Binary(),
                            Placement_ID = c.Long()
                        })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Devices", t => t.DeviceID, true)
                .ForeignKey("dbo.SensorTypes", t => t.SensorTypeID, true)
                .ForeignKey("dbo.Placements", t => t.Placement_ID)
                .Index(t => t.SensorTypeID)
                .Index(t => t.DeviceID)
                .Index(t => t.Placement_ID);

            CreateTable("dbo.SensorHistories",
                    c =>
                        new
                        {
                            SensorID = c.Guid(false),
                            TimeStamp = c.DateTimeOffset(false, 7),
                            LocationID = c.Guid(),
                            RawData = c.Binary()
                        })
                .PrimaryKey(t => new {t.SensorID, t.TimeStamp})
                .ForeignKey("dbo.Sensors", t => t.SensorID, true)
                .ForeignKey("dbo.Locations", t => t.LocationID)
                .Index(t => t.SensorID)
                .Index(t => t.LocationID);

            CreateTable("dbo.People",
                c =>
                    new
                    {
                        ID = c.Guid(false),
                        TwitterHandle = c.String(),
                        UserName = c.String(),
                        CreatedAt = c.DateTimeOffset(false, 7),
                        UpdatedAt = c.DateTimeOffset(false, 7)
                    }).PrimaryKey(t => t.ID);
        }
    }
}
