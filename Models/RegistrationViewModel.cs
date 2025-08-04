using System.ComponentModel.DataAnnotations;

namespace UserManagementApp.Models
{
    public class RegistrationViewModel
    {
        [Required]
        public required string Name { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}