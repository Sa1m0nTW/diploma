using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace WorkWise.ViewModels
{
    public class GoalEditViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        public string Name { get; set; }

        public string Desc { get; set; }

        [Range(1, 5, ErrorMessage = "Важность должна быть от 1 до 5")]
        public int Importancy { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime FinishTime { get; set; }

        [Display(Name = "Исполнители")]
        public List<string> SelectedPerformerIds { get; set; } = new List<string>();

        public Guid SquadId { get; set; }
        public bool IsCompleted { get; set; }
        public List<PerformerViewModel> AvailablePerformers { get; set; } = new List<PerformerViewModel>();

    }
}
