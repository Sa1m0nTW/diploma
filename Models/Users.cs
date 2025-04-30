using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkWise.Models
{
    public class Users : IdentityUser
    {
        public string? FullName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Bio { get; set; }

        public ICollection<UserSquad> UserSquads { get; set; }
        public ICollection<Goals> Goals { get; set; }
    }
}
