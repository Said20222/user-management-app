using System.ComponentModel.DataAnnotations;

namespace UserManagementApp.Models
{
    public class LoginViewModel
    {

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public required string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; } = string.Empty;
    }
}