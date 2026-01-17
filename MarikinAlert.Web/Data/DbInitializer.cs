using MarikinAlert.Web.Models;
using System.Linq;

namespace MarikinAlert.Web.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // Check if any admin exists
            if (context.AdminUsers.Any()) return;

            // Create the default admin
            var admin = new AdminUser
            {
                Username = "admin",
                Password = "admin123",      // For simple login
                PasswordHash = "admin123",  // For the legacy code
                FullName = "System Administrator",
                Role = "HeadDispatcher",
                AssignedNodeName = "Central Command"
            };

            context.AdminUsers.Add(admin);
            context.SaveChanges();
        }
    }
}