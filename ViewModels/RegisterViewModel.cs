using System.ComponentModel.DataAnnotations;

namespace WorkWise.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Необходимо укзаать Имя")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Необходимо укзаать Email")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Необходимо укзаать пароль")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Ваш пароль должен содержать от {2} до {1} символов")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Пароли должны совпадать")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Необходимо ввести пароль ещё раз")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтвердите пароль")]
        public string ConfirmPassword { get; set; }
    }
}
