namespace GhAPIAzure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GreenhouseKeyProper : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CropCycles", "Name", "dbo.CropTypes");
            DropIndex("dbo.CropCycles", new[] { "Name" });
            AddColumn("dbo.CropCycles", "CropTypeName", c => c.String(nullable: false, maxLength: 245));
            AlterColumn("dbo.CropCycles", "Name", c => c.String());
            CreateIndex("dbo.CropCycles", "CropTypeName");
            AddForeignKey("dbo.CropCycles", "CropTypeName", "dbo.CropTypes", "Name", cascadeDelete: true);
            DropColumn("dbo.CropCycles", "CropTypeID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CropCycles", "CropTypeID", c => c.Guid(nullable: false));
            DropForeignKey("dbo.CropCycles", "CropTypeName", "dbo.CropTypes");
            DropIndex("dbo.CropCycles", new[] { "CropTypeName" });
            AlterColumn("dbo.CropCycles", "Name", c => c.String(maxLength: 245));
            DropColumn("dbo.CropCycles", "CropTypeName");
            CreateIndex("dbo.CropCycles", "Name");
            AddForeignKey("dbo.CropCycles", "Name", "dbo.CropTypes", "Name");
        }
    }
}
