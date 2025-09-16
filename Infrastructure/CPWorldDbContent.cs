using CpWorld.Models;
using Microsoft.EntityFrameworkCore;

namespace CpWorld.Infrastructure
{
    public class CPWorldDbContent : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(@"Data Source=localhost;Initial Catalog=cp_world_db;Integrated Security=SSPI;Encrypt=false;");
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Item);
        }

        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Item> Item { get; set; }
    }
}
