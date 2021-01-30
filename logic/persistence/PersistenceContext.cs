using System.IO;
using Fork.Logic.Model;
using Fork.Logic.Model.ProxyModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fork.Logic.Persistence
{
    public class PersistenceContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<Network> Networks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
            optionsBuilder.UseSqlite("foreign keys=true;Data Source=" +
                                     Path.Combine(App.ApplicationPath, "persistence", "data.db"));
#if DEBUG
            optionsBuilder.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()));
#endif

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*var restrictFks = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership);
            foreach (var restrictFk in restrictFks)
            {
                restrictFk.DeleteBehavior = DeleteBehavior.Cascade;
            }*/

            base.OnModelCreating(modelBuilder);
        }
    }
}