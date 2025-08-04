using System.ComponentModel.DataAnnotations;

namespace UserManagementApp.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required, EmailAddress]
        public required string Email { get; set; }
        [Required]
        public required string PasswordHash { get; set; }
        public bool IsBlocked { get; set; } = false;
        public DateTime? RegisteredAt { get; set; } = null;
        public DateTime? LastLoginAt { get; set; } = null;
    }
}