using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace WorkWise.ViewModels
{
    public class GoalCreateViewModel
    {
        [Required(ErrorMessage = "Название обязательно")]
        public string Name { get; set; }

        public string Desc { get; set; }

        [Range(1, 5, ErrorMessage = "Важность должна быть от 1 до 5")]
        public int Importancy { get; set; } = 3;

        [DataType(DataType.DateTime)]
        [Display(Name = "Срок выполнения")]
        public DateTime FinishTime { get; set; } = DateTime.UtcNow.AddDays(7);

        [Display(Name = "Исполнитель")]
        public string? PerformerID { get; set; }

        [Required]
        public Guid SquadId { get; set; }
        public List<SelectListItem>? AvailablePerformers { get; set; }

    }
}
