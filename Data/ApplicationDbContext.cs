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
        public DbSet<AgencyPoint> AgencyPoints { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Travel> Travels { get; set; }
        public DbSet<Client> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Agency configurations
            builder.Entity<Agency>(entity =>
            {
                entity.HasIndex(a => a.Name).IsUnique();
                entity.HasMany(a => a.AgencyPoints)
                      .WithOne(ap => ap.Agency)
                      .HasForeignKey(ap => ap.AgencyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AgencyPoint configurations
            builder.Entity<AgencyPoint>(entity =>
            {
                entity.HasIndex(ap => new { ap.Name, ap.AgencyId }).IsUnique();
            });

            // ApplicationUser configurations
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne(u => u.Agency)
                      .WithMany(a => a.Users)
                      .HasForeignKey(u => u.AgencyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.AgencyPoint)
                      .WithMany(ap => ap.Users)
                      .HasForeignKey(u => u.AgencyPointId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.CreatedBy)
                      .WithMany()
                      .HasForeignKey(u => u.CreatedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Travel configurations
            builder.Entity<Travel>(entity =>
            {
                entity.HasOne(t => t.Agency)
                      .WithMany(a => a.Travels)
                      .HasForeignKey(t => t.AgencyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.AgencyPoint)
                      .WithMany(ap => ap.Travels)
                      .HasForeignKey(t => t.AgencyPointId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Reservation configurations
            builder.Entity<Reservation>(entity =>
            {
                entity.HasOne(r => r.Agency)
                      .WithMany(a => a.Reservations)
                      .HasForeignKey(r => r.AgencyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.AgencyPoint)
                      .WithMany(ap => ap.Reservations)
                      .HasForeignKey(r => r.AgencyPointId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.ConfirmedBy)
                      .WithMany()
                      .HasForeignKey(r => r.ConfirmedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
