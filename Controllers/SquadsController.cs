using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkWise.Data;
using WorkWise.Models;

namespace WorkWise.Controllers
{
    [Authorize] // Только для авторизованных пользователей
    [Route("api/[controller]")]
    [ApiController]
        public class SquadsController : ControllerBase
        {
            private readonly AppDbContext _context;
            private readonly UserManager<Users> _userManager;

            public SquadsController(AppDbContext context, UserManager<Users> userManager)
            {
                _context = context;
                _userManager = userManager;
            }

            [HttpPost("create")]
            public async Task<IActionResult> CreateSquad([FromBody] SquadCreateModel model)
            {
                // Получаем текущего пользователя
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                // Создаем новую команду
                var squad = new Squads
                {
                    Name = model.Name,
                    Leader = currentUser,
                    LeaderID = currentUser.Id
                };

                // Добавляем создателя в участники команды
                var userSquad = new UserSquad
                {
                    User = currentUser,
                    Squad = squad,
                    Role = "Leader",
                    JoinedAt = DateTime.UtcNow
                };

                _context.Squads.Add(squad);
                _context.UserSquads.Add(userSquad);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    squad.Id,
                    squad.Name,
                    Leader = new { currentUser.Id, currentUser.UserName }
                });
            }
        }

        public class SquadCreateModel
        {
            public string Name { get; set; }
        }
    }
