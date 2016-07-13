namespace GhAPIAzure.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class HistoryUploadedAt : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.SensorHistories", "UpdatedAt", "UploadedAt");
        }

        public override void Down()
        {
            RenameColumn("dbo.SensorHistories", "UploadedAt", "UpdatedAt");
        }
    }
}
