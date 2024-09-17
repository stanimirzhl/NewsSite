using System.ComponentModel.DataAnnotations;

namespace NewsSite.Models.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Въведете парола")]
        [DataType(DataType.Password, ErrorMessage = "Неправилен формат за парола")]
        public string Password { get; set; }

       public string? ReturnUrl { get; set; }
    }
}
