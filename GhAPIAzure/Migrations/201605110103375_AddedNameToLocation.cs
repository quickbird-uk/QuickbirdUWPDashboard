namespace GhAPIAzure.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddedNameToLocation : DbMigration
    {
        public override void Down() { DropColumn("dbo.Locations", "Name"); }

        public override void Up() { AddColumn("dbo.Locations", "Name", c => c.String(false)); }
    }
}
