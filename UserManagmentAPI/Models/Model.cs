using System.ComponentModel.DataAnnotations;

namespace UserManagmentAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [RegularExpression("^(HR|IT)$", ErrorMessage = "Department must be HR or IT")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }
}
