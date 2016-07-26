namespace GhAPIAzure.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AlertsOptional : DbMigration
    {
        public override void Down()
        {
            AlterColumn("dbo.Sensors", "AlertLow", c => c.Double(false));
            AlterColumn("dbo.Sensors", "AlertHigh", c => c.Double(false));
        }

        public override void Up()
        {
            AlterColumn("dbo.Sensors", "AlertHigh", c => c.Double());
            AlterColumn("dbo.Sensors", "AlertLow", c => c.Double());
        }
    }
}
