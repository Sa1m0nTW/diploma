namespace WorkWise.Models
{
    public class Squads
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Users Leader { get; set; }
        public string LeaderID { get; set; }

        public ICollection<UserSquad> UserSquads { get; set; }
        public ICollection<Goals> Goals { get; set; } 
    }
}
