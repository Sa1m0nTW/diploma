using System.ComponentModel.DataAnnotations;

namespace WorkWise.ViewModels
{
    public class VerifyEmailViewModel
    {
        [Required(ErrorMessage = "Необходимо укзаать Email")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
