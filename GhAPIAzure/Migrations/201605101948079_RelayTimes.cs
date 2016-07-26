namespace GhAPIAzure.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class RelayTimes : DbMigration
    {
        public override void Down()
        {
            DropColumn("dbo.Relays", "OffTime");
            DropColumn("dbo.Relays", "OnTime");
        }

        public override void Up()
        {
            AddColumn("dbo.Relays", "OnTime", c => c.Int(false));
            AddColumn("dbo.Relays", "OffTime", c => c.Int(false));
        }
    }
}
