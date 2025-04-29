namespace WorkWise.ViewModels
{
    public class SquadsDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LeaderName { get; set; }
        public bool IsCurrentUserLeader { get; set; }
        public List<MemberViewModel> Members { get; set; }
    }

    public class MemberViewModel
    {
        public string UserName { get; set; }
        public string Role { get; set; }
    }
}
