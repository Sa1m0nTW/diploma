namespace WorkWise.Models
{
    public class UserSquad
    {
        public string UserId { get; set; }
        public Users User { get; set; }

        public Guid SquadId { get; set; }
        public Squads Squad { get; set; }

        public string? Role { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
