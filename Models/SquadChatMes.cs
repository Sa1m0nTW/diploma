namespace WorkWise.Models
{
    public class SquadChatMes
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AuthorId { get; set; }
        public Users Author { get; set; }
        public Guid SquadId { get; set; }
        public Squads Squad { get; set; }
    }
}
