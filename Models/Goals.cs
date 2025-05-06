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
        public virtual ICollection<Users> Performers { get; set; } = new List<Users>();
        public Squads Squad { get; set; }
        public Guid SquadId { get; set; }

        public ICollection<GoalFeedback>? Feedbacks { get; set; }
        public ICollection<GoalAttachments> Attachments { get; set; }
    }
}
