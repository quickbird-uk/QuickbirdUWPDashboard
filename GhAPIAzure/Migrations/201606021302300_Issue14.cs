namespace GhAPIAzure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Issue14 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CropCycles", "Yield", c => c.Double(nullable: false));
            AddColumn("dbo.CropCycles", "CropVariety", c => c.String(nullable: false));
            AddColumn("dbo.Sensors", "Alarmed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sensors", "Alarmed");
            DropColumn("dbo.CropCycles", "CropVariety");
            DropColumn("dbo.CropCycles", "Yield");
        }
    }
}
