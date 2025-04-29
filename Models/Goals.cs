namespace WorkWise.Models
{
    public class Goals
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }

        public int Importancy { get; set; }
        public bool State { get; set; }
        public DateTime FinishTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public Users? Performer { get; set; }
        public string? PerformerID { get; set; }
        public Squads Squad { get; set; }
        public Guid SquadId { get; set; }

        public ICollection<GoalFeedback>? Feedbacks { get; set; }
        public ICollection<GoalAttachments> Attachments { get; set; }
    }
}
