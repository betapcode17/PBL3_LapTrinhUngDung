using System.ComponentModel.DataAnnotations;

namespace Volunteer_website.ViewModel
{
    public class LoginVM
    {
        [Required(ErrorMessage ="Username cannot be blank.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Password cannot be blank.")]
        public string Password { get; set; }
    }
}
