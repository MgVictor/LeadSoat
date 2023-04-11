using LeadSoatApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeadSoatApi.Data
{
    public class ConfigCampaignDbContext : DbContext
    {
        public ConfigCampaignDbContext(DbContextOptions<ConfigCampaignDbContext> options) : base(options)
        {

        }
        public DbSet<configCampaignTest>? Test { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConfigCampaign>().HasNoKey();
        }
    }
}
