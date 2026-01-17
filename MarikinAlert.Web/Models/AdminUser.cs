using System.ComponentModel.DataAnnotations;

namespace MarikinAlert.Web.Models
{
    public class AdminUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        // Both password fields are here to satisfy all legacy code
        public string Password { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Role { get; set; } = "Dispatcher";

        public string AssignedNodeName { get; set; } = "Central Command";
    }
}