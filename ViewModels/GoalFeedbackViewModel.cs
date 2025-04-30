using System.ComponentModel.DataAnnotations;

namespace WorkWise.ViewModels
{
    public class GoalFeedbackViewModel
    {
        [Required(ErrorMessage = "Отзыв не может быть пустым")]
        [StringLength(500, ErrorMessage = "Отзыв должен быть до 500 символов")]
        public string Comment { get; set; }

        public Guid GoalId { get; set; }
        public string GoalName { get; set; }
        [Display(Name = "Отметить задачу выполненной")]
        public bool MarkAsCompleted { get; set; }
        public bool IsLeader { get; set; }
    }
}
