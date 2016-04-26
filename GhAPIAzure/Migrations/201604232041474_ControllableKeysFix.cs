namespace GhAPIAzure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ControllableKeysFix : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Controllables", "ControlType_ID", "dbo.ControlTypes");
            DropIndex("dbo.Controllables", new[] { "ControlType_ID" });
            DropColumn("dbo.Controllables", "ControlTypeID");
            RenameColumn(table: "dbo.Controllables", name: "ControlType_ID", newName: "ControlTypeID");
            AlterColumn("dbo.Controllables", "ControlTypeID", c => c.Long(nullable: false));
            AlterColumn("dbo.Controllables", "ControlTypeID", c => c.Long(nullable: false));
            CreateIndex("dbo.Controllables", "ControlTypeID");
            AddForeignKey("dbo.Controllables", "ControlTypeID", "dbo.ControlTypes", "ID", cascadeDelete: true);
            DropColumn("dbo.Relays", "ControllableID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Relays", "ControllableID", c => c.Guid(nullable: false));
            DropForeignKey("dbo.Controllables", "ControlTypeID", "dbo.ControlTypes");
            DropIndex("dbo.Controllables", new[] { "ControlTypeID" });
            AlterColumn("dbo.Controllables", "ControlTypeID", c => c.Long());
            AlterColumn("dbo.Controllables", "ControlTypeID", c => c.Guid(nullable: false));
            RenameColumn(table: "dbo.Controllables", name: "ControlTypeID", newName: "ControlType_ID");
            AddColumn("dbo.Controllables", "ControlTypeID", c => c.Guid(nullable: false));
            CreateIndex("dbo.Controllables", "ControlType_ID");
            AddForeignKey("dbo.Controllables", "ControlType_ID", "dbo.ControlTypes", "ID");
        }
    }
}
