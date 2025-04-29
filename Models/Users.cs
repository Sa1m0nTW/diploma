using Microsoft.AspNetCore.Identity;

namespace WorkWise.Models
{
    public class Users : IdentityUser
    {
        public string? FullName { get; set; }

        public ICollection<UserSquad> UserSquads { get; set; }
        public ICollection<Goals> Goals { get; set; }
    }
}
