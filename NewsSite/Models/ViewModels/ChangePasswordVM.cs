using System.ComponentModel.DataAnnotations;

namespace NewsSite.Models.ViewModels
{
    public class ChangePasswordVM
    {
        [Required(ErrorMessage ="Полето е задължително")]
        [StringLength(25, ErrorMessage = "Паролата трябва да е между {2} и {1} символа", MinimumLength = 5)]
        [DataType(DataType.Password, ErrorMessage = "Неправилен формат за парола")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "Полето е задължително")]
        [StringLength(25, ErrorMessage = "Паролата трябва да е между {2} и {1} символа", MinimumLength = 5)]
        [DataType(DataType.Password, ErrorMessage = "Неправилен формат за парола")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Полето е задължително")]
        [DataType(DataType.Password, ErrorMessage = "Неправилен формат за парола")]
        [Compare("NewPassword", ErrorMessage = "Паролите не съвпадат")]
        [StringLength(25, ErrorMessage = "Паролата трябва да е между {2} и {1} символа", MinimumLength = 5)]
        public string NewPasswordConfirm { get; set; }
    }
}
