using LeadSoatApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeadSoatApi.Data
{
    public class LeadSoatDbContext : DbContext
    {
        public LeadSoatDbContext(DbContextOptions<LeadSoatDbContext> options) : base(options)
        {

        }
        public DbSet<LeadSoatTest>? Test { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Lead>().HasNoKey();
        }
    }
}
