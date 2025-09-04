using CpWorld.Models;
using Microsoft.EntityFrameworkCore;

namespace CpWorld.Infrastructure
{
    public class CPWorldDbContent : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(@"Data Source=DESKTOP-BOCE6T9;Initial Catalog=cp_world_db;Integrated Security=SSPI;Encrypt=false;");
        }

        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Order> Orders { get; set; }
    }
}
