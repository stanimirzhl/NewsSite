using System.ComponentModel.DataAnnotations;

namespace NewsSite.Models.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Полето не може да остане празно")]
        [EmailAddress(ErrorMessage = "Електронната поща не е в правилния формат")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Паролата не може да остане празна")]
        [StringLength(25, ErrorMessage = "Паролата трябва да е между {2} и {1} символа", MinimumLength = 5)]
        [DataType(DataType.Password, ErrorMessage = "Неправилен формат за парола")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Паролата не може да остане празна")]
        [DataType(DataType.Password, ErrorMessage = "Неправилен формат за парола")]
        [Compare("Password", ErrorMessage = "Паролите не съвпадат")]
        [StringLength(25, ErrorMessage = "Паролата трябва да е между {2} и {1} символа", MinimumLength = 5)]
        public string ConfirmPassword { get; set; }
    }
}
