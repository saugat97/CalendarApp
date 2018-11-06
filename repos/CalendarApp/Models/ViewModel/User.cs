using System.ComponentModel.DataAnnotations;

namespace CalendarApp.Models
{
    [MetadataType(typeof(UserData))]
    public partial class User
    {
        public string ConfirmPassword { get; set; }
    }

    public class UserData
    {
        [Display(Name = "First Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last name is required")]
        public string LastName { get; set; }

        [Display(Name = "Email address")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Your email address is required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required for registration")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Minimum 6 char required!")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Your initial Password didn't match!")]
        public string ConfirmPassword { get; set; }
    }
}