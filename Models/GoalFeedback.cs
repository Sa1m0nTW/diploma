namespace WorkWise.Models
{
    public class GoalFeedback
    {
        public Guid Id { get; set; }
        public string Comment { get; set; }      
        public DateTime CreatedAt { get; set; }
        public Guid GoalId { get; set; }        
        public Goals Goal { get; set; }          

        public string AuthorId { get; set; } 
        public Users Author { get; set; }

        public bool IsLeaderComment { get; set; } = false;
    }
}
