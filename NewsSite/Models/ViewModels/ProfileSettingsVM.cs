using System.ComponentModel.DataAnnotations;

namespace NewsSite.Models.ViewModels
{
    public class ProfileSettingsVM
    {
        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Полето не може да остане празно")]
        [EmailAddress(ErrorMessage = "Електронната поща не е в правилния формат")]
        public string Email { get; set; }
    }
}
