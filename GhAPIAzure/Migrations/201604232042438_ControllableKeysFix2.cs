namespace GhAPIAzure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ControllableKeysFix2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Controllables", "Relay_ID", "dbo.Relays");
            DropIndex("dbo.Controllables", new[] { "Relay_ID" });
            AddColumn("dbo.Controllables", "Relay_ID1", c => c.Guid());
            CreateIndex("dbo.Controllables", "Relay_ID1");
            AddForeignKey("dbo.Controllables", "Relay_ID1", "dbo.Relays", "ID");
            DropColumn("dbo.Controllables", "RelayID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Controllables", "RelayID", c => c.Guid());
            DropForeignKey("dbo.Controllables", "Relay_ID1", "dbo.Relays");
            DropIndex("dbo.Controllables", new[] { "Relay_ID1" });
            DropColumn("dbo.Controllables", "Relay_ID1");
            CreateIndex("dbo.Controllables", "Relay_ID");
            AddForeignKey("dbo.Controllables", "Relay_ID", "dbo.Relays", "ID");
        }
    }
}
