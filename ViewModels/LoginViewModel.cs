using System.ComponentModel.DataAnnotations;

namespace WorkWise.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Необходимо ввести Email")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Необходимо ввести пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Запомнить меня?")]
        public bool RememberMe { get; set; }
    }
}
