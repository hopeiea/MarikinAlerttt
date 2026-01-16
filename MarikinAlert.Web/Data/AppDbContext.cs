using MarikinAlert.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace MarikinAlert.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // 1. ACTIVE REPORTS
        public DbSet<DisasterReport> Reports { get; set; }
        
        // 2. ARCHIVED REPORTS (This was missing!)
        public DbSet<ArchivedReport> ArchivedReports { get; set; }

        // 3. ADMIN USERS
        public DbSet<AdminUser> AdminUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Report Config
            modelBuilder.Entity<DisasterReport>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RawMessage).IsRequired();
                
                // DELETED: Latitude and Longitude configuration lines are gone.
                // This fixes the "does not contain a definition for Latitude" error.
            });

            // Seed Admin User
            modelBuilder.Entity<AdminUser>().HasData(
                new AdminUser
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "marikina123",
                    AssignedNodeName = "MasterNode"
                }
            );
        }
    }
}