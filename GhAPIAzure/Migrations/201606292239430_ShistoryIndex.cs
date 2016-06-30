namespace GhAPIAzure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ShistoryIndex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.SensorHistories", "UpdatedAt");
        }
        
        public override void Down()
        {
            DropIndex("dbo.SensorHistories", new[] { "UpdatedAt" });
        }
    }
}
