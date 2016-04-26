namespace GhAPIAzure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NUkedanavproperty : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Controllables", name: "Relay_ID1", newName: "RelayID");
            RenameIndex(table: "dbo.Controllables", name: "IX_Relay_ID1", newName: "IX_RelayID");
            DropColumn("dbo.Controllables", "Relay_ID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Controllables", "Relay_ID", c => c.Guid());
            RenameIndex(table: "dbo.Controllables", name: "IX_RelayID", newName: "IX_Relay_ID1");
            RenameColumn(table: "dbo.Controllables", name: "RelayID", newName: "Relay_ID1");
        }
    }
}
