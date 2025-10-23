using ErpBE.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ErpBE.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Define your DbSets here
        public DbSet<UnitMaster> UnitMasters { get; set; }

        // OnModelCreating method is used for additional configurations (e.g., relationships, constraints, etc.)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API configurations (optional, based on needs)
        }
    }

}
