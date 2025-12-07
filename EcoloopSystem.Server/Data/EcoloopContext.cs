using Microsoft.EntityFrameworkCore;
using EcoloopSystem.Server.Models;

namespace EcoloopSystem.Server.Data
{
    public class EcoloopContext : DbContext
    {
        public EcoloopContext(DbContextOptions<EcoloopContext> options) : base(options) { }

        public DbSet<EcoloopSystem.Server.Models.User> Users { get; set; }
        public DbSet<Tableware> Tablewares { get; set; }
        public DbSet<Rental> Rentals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EcoloopSystem.Server.Models.User>().HasIndex(u => u.CardId).IsUnique();
            modelBuilder.Entity<Tableware>().HasIndex(t => t.TagId).IsUnique();
        }
    }
}
