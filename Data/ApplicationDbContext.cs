using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TravelSaaS.Models.Entities;

namespace TravelSaaS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>

    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Travel> Travels { get; set; }
        public DbSet<Client> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurations supplémentaires
            builder.Entity<Agency>(entity =>
            {
                entity.HasIndex(a => a.Name).IsUnique();
            });
        }
    }
}
