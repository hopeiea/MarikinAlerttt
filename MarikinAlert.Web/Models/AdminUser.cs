using System.ComponentModel.DataAnnotations;

namespace MarikinAlert.Web.Models 
{
    public class AdminUser
    {
        [Key]
        public int Id { get; set; }
        
        public string Username { get; set; } = string.Empty;
        
        public string PasswordHash { get; set; } = string.Empty;
        
        public string? AssignedNodeName { get; set; }
    }
}