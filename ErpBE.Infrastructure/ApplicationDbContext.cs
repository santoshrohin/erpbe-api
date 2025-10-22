using ErpBE.Domain.FarmerEntities;
using Microsoft.EntityFrameworkCore;

namespace ErpBE.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Define your DbSets here
        public DbSet<BranchMaster> BranchMasters { get; set; }
        public DbSet<LineMaster> LineMasters { get; set; }
        public DbSet<FarmerMaster> FarmerMasters { get; set; }
        public DbSet<FarmerItemMaster> FarmerItemMasters { get; set; }
        public DbSet<Placement> Placements { get; set; }
        public DbSet<PlacementDetail> PlacementDetails { get; set; }

        // OnModelCreating method is used for additional configurations (e.g., relationships, constraints, etc.)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API configurations (optional, based on needs)
        }
    }

}
