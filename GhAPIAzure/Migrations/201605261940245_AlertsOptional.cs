namespace GhAPIAzure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlertsOptional : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Sensors", "AlertHigh", c => c.Double());
            AlterColumn("dbo.Sensors", "AlertLow", c => c.Double());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Sensors", "AlertLow", c => c.Double(nullable: false));
            AlterColumn("dbo.Sensors", "AlertHigh", c => c.Double(nullable: false));
        }
    }
}
