using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkWise.Models;

namespace WorkWise.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Squads> Squads { get; set; }
        public DbSet<Goals> Goals { get; set; }
        public DbSet<GoalFeedback> GoalFeedbacks { get; set; }
        public DbSet<GoalAttachments> GoalAttachments { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<UserSquad> UserSquads { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(x => new { x.LoginProvider, x.ProviderKey });
            modelBuilder.Entity<IdentityUserRole<string>>().HasKey(x => new { x.UserId, x.RoleId });
            modelBuilder.Entity<IdentityUserToken<string>>().HasKey(x => new { x.UserId, x.LoginProvider, x.Name });
            modelBuilder.Entity<UserSquad>()
                .HasKey(us => new { us.UserId, us.SquadId });

            modelBuilder.Entity<UserSquad>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserSquads)
                .HasForeignKey(us => us.UserId);

            modelBuilder.Entity<UserSquad>()
                .HasOne(us => us.Squad)
                .WithMany(t => t.UserSquads)
                .HasForeignKey(us => us.SquadId);
            }
    }
    }
