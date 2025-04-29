namespace WorkWise.Models
{
    public class Invitation
    {
        public Guid Id { get; set; }
        public string InvitedUserId { get; set; }
        public Users InvitedUser { get; set; }
        public string InviterUserId { get; set; }
        public Users InviterUser { get; set; }
        public Guid SquadId { get; set; }
        public Squads Squad { get; set; }
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
        public string? Message { get; set; }
        public string? Role { get; set; }
    }

    public enum InvitationStatus
    {
        Pending, 
        Accepted,   
        Rejected,   
        Cancelled   
    }
}
