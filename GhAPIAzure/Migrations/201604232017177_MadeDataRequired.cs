namespace GhAPIAzure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MadeDataRequired : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CropCycles", "Name", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CropCycles", "Name", c => c.String());
        }
    }
}
