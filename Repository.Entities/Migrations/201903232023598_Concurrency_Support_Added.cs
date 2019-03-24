using System.Data.Entity.Migrations;

namespace Repository.Entities.Migrations
{
    public partial class Concurrency_Support_Added : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Links", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
            AddColumn("dbo.Links", "UpdatedAt", c => c.DateTimeOffset(nullable: false, precision: 7));
            AlterColumn("dbo.Domains", "Name", c => c.String(maxLength: 255));
            AlterColumn("dbo.MediaServices", "Name", c => c.String(maxLength: 255));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.MediaServices", "Name", c => c.String());
            AlterColumn("dbo.Domains", "Name", c => c.String());
            DropColumn("dbo.Links", "UpdatedAt");
            DropColumn("dbo.Links", "RowVersion");
        }
    }
}
