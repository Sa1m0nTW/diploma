namespace WorkWise.Models
{
    public class ChangeRoleViewModel
    {
        public string UserId { get; set; }
        public Guid SquadId { get; set; }
        public string NewRole { get; set; }
        public List<string> AvailableRoles { get; set; } = new List<string> { "Member", "Moderator" }; // Пример ролей
    }
}