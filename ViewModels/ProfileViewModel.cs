using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace WorkWise.ViewModels
{
    public class ProfileViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Полное имя")]
        public string FullName { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Фотография профиля")]
        public IFormFile? ProfilePicture { get; set; }
        public string? ProfilePictureUrl { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Дата рождения")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "О себе")]
        [StringLength(500)]
        public string? Bio { get; set; }

        public bool IsCurrentUser { get; set; }
    }
}
