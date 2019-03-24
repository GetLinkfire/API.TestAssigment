using System.Data.Entity;

namespace Repository.Entities
{
    // I would put the DbContext implemetation within the same assembly with Entities
    // Because you create entities in the separate assembly to have your Model defined in one place
    // And probably be later used in other solutions/project so it should not contain any specific
    // dependencies like EntityFramework
    // I think Repository assembly here suits best since it is the particular implementation of Data Access layer
	public class LinksContext : DbContext
	{
        static LinksContext()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<LinksContext, Migrations.Configuration>());
        }

        public LinksContext() : base("name=LocalConnectionString")
        {
        }

		public DbSet<Link> Links { get; set; }

		public DbSet<Domain> Domains { get; set; }

		public DbSet<Artist> Artists { get; set; }

		public DbSet<MediaService> MediaServices { get; set; }
	}
}
