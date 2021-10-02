using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace CloudConfiguation.DB
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext()
            : base("name=MySQLConnection")
        {
            // Get the ObjectContext related to this DbContext
            // var objectContext = (this as IObjectContextAdapter).ObjectContext;
            Database.SetInitializer<DatabaseContext>(null);
            //// Sets the command timeout for all the commands
            //objectContext.CommandTimeout = int.MaxValue;
        }

        public DbSet<TblConfiguration> Configuration { get; set; }
        public DbSet<ManifestInfo> ManifestInfo { get; set; }
        public DbSet<UserBuildMaster> UserBuildMaster { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }
    }
}
