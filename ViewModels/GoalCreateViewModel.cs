using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace WorkWise.ViewModels
{
    public class GoalCreateViewModel
    {
        [Required(ErrorMessage = "Название обязательно")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Опишите суть задачи")]
        public string Desc { get; set; }

        [Range(1, 5, ErrorMessage = "Важность должна быть от 1 до 5")]
        public int Importancy { get; set; } = 3;

        [DataType(DataType.DateTime)]
        [Display(Name = "Срок выполнения")]
        public DateTime FinishTime { get; set; } = DateTime.UtcNow.AddDays(7);

        [Display(Name = "Исполнители")]
        public List<string>? SelectedPerformerIds { get; set; } = new List<string>();

        [Required]
        public Guid SquadId { get; set; }
        public List<PerformerViewModel> AvailablePerformers { get; set; } = new List<PerformerViewModel>();

    }
    public class PerformerViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}
