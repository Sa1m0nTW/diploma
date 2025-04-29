using System.ComponentModel.DataAnnotations;

namespace WorkWise.Models
{
    public class SquadsCreateModel
    {
        [Required(ErrorMessage = "Название команды обязательно")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Название должно быть от 3 до 100 символов")]
        public string Name { get; set; }
    }
}