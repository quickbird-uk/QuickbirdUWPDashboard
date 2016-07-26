namespace GhAPIAzure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedNameToLocation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Locations", "Name", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Locations", "Name");
        }
    }
}
