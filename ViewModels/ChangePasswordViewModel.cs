using System.ComponentModel.DataAnnotations;

namespace WorkWise.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Необходимо укзаать Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Необходимо укзаать пароль")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Ваш {0} должен содержать от {2} до {1} символов")]
        [DataType(DataType.Password)]
        [Display(Name = "Введите новый пароль")]
        [Compare("ConfirmNewPassword", ErrorMessage = "Пароли должны совпадать")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Необходимо ввести пароль ещё раз")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтвердите новый пароль")]
        public string ConfirmNewPassword { get; set; }
    }
}
