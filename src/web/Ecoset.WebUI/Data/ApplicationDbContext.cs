using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ecoset.WebUI.Models;

namespace Ecoset.WebUI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Job> Jobs {get; set; }
        public DbSet<PriceThreshold> PriceThresholds {get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<ShoppingBasket> ShoppingBaskets { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ApplicationUser>().HasKey(l => l.Id);
            base.OnModelCreating(builder);
        }
    }
}
