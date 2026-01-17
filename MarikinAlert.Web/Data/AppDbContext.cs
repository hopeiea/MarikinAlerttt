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

        // 4. GOVERNMENT AGENCIES
        public DbSet<GovernmentAgency> GovernmentAgencies { get; set; }

        // 5. AGENCY RELAYS
        public DbSet<AgencyRelay> AgencyRelays { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Report Config with Indexes
            modelBuilder.Entity<DisasterReport>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RawMessage).IsRequired();
                
                // Performance indexes for common queries
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.Category, e.Priority }); // Composite for filtering
                entity.HasIndex(e => new { e.Priority, e.Timestamp }); // Composite for sorting
            });

            // Archived Report Indexes
            modelBuilder.Entity<ArchivedReport>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ArchivedTimestamp);
                entity.HasIndex(e => e.OriginalReportId);
                entity.HasIndex(e => e.Status);
            });

            // Admin User Indexes
            modelBuilder.Entity<AdminUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Government Agency Indexes
            modelBuilder.Entity<GovernmentAgency>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.Acronym);
            });

            // Agency Relay Indexes
            modelBuilder.Entity<AgencyRelay>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ReportId);
                entity.HasIndex(e => e.AgencyId);
                entity.HasIndex(e => e.RelayedAt);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.ReportId, e.AgencyId });
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

            // Seed Government Agencies
            modelBuilder.Entity<GovernmentAgency>().HasData(
                new GovernmentAgency
                {
                    Id = 1,
                    Name = "Bureau of Fire Protection",
                    Acronym = "BFP",
                    Description = "Philippines national fire fighting agency",
                    EmergencyHotline = "116",
                    Email = "bfp@fire.gov.ph",
                    Address = "BFP National Headquarters, Quezon City",
                    HandlesCategories = "Fire",
                    IsActive = true,
                    Priority = 1
                },
                new GovernmentAgency
                {
                    Id = 2,
                    Name = "Philippine National Police",
                    Acronym = "PNP",
                    Description = "National police force for emergency response",
                    EmergencyHotline = "117",
                    Email = "hotline@pnp.gov.ph",
                    Address = "PNP National Headquarters, Camp Crame",
                    HandlesCategories = "BuildingCollapse,Infrastructure",
                    IsActive = true,
                    Priority = 1
                },
                new GovernmentAgency
                {
                    Id = 3,
                    Name = "Philippine Red Cross",
                    Acronym = "PRC",
                    Description = "Emergency medical services and disaster response",
                    EmergencyHotline = "143",
                    Email = "info@redcross.org.ph",
                    Address = "PRC National Headquarters, Manila",
                    HandlesCategories = "Medical",
                    IsActive = true,
                    Priority = 1
                },
                new GovernmentAgency
                {
                    Id = 4,
                    Name = "Metro Manila Development Authority",
                    Acronym = "MMDA",
                    Description = "Metro Manila traffic and emergency coordination",
                    EmergencyHotline = "136",
                    Email = "mmda@mmda.gov.ph",
                    Address = "MMDA Main Office, EDSA, Quezon City",
                    HandlesCategories = "Infrastructure,Logistics",
                    IsActive = true,
                    Priority = 2
                },
                new GovernmentAgency
                {
                    Id = 5,
                    Name = "Office of Civil Defense",
                    Acronym = "OCD",
                    Description = "National disaster risk reduction and management",
                    EmergencyHotline = "911",
                    Email = "ocd@ocd.gov.ph",
                    Address = "OCD National Headquarters, Quezon City",
                    HandlesCategories = "Fire,BuildingCollapse,Medical,Logistics,Infrastructure",
                    IsActive = true,
                    Priority = 1
                },
                new GovernmentAgency
                {
                    Id = 6,
                    Name = "Department of Public Works and Highways",
                    Acronym = "DPWH",
                    Description = "Infrastructure and public works emergency response",
                    EmergencyHotline = "165-02",
                    Email = "info@dpwh.gov.ph",
                    Address = "DPWH Central Office, Port Area, Manila",
                    HandlesCategories = "Infrastructure,BuildingCollapse",
                    IsActive = true,
                    Priority = 2
                },
                new GovernmentAgency
                {
                    Id = 7,
                    Name = "City Health Office - Marikina",
                    Acronym = "CHO",
                    Description = "Local health department for medical emergencies",
                    EmergencyHotline = "8888",
                    Email = "health@marikina.gov.ph",
                    Address = "Marikina City Health Office, Marikina City",
                    HandlesCategories = "Medical",
                    IsActive = true,
                    Priority = 2
                },
                new GovernmentAgency
                {
                    Id = 8,
                    Name = "City Engineering Office - Marikina",
                    Acronym = "CEO",
                    Description = "Local engineering office for infrastructure emergencies",
                    EmergencyHotline = "8888",
                    Email = "engineering@marikina.gov.ph",
                    Address = "Marikina City Engineering Office, Marikina City",
                    HandlesCategories = "Infrastructure,BuildingCollapse",
                    IsActive = true,
                    Priority = 3
                }
            );
        }
    }
}