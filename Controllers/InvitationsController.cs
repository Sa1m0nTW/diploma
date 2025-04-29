using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkWise.Data;
using WorkWise.Models;

namespace WorkWise.Controllers
{
    [Authorize]
    public class InvitationsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public InvitationsController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Отправка приглашения
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInvitationDto dto)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var invitedUser = await _userManager.FindByNameAsync(dto.InvitedUserName);

                if (invitedUser == null)
                    return Json(new { success = false, message = "Пользователь не найден" });

                if (await _context.UserSquads.AnyAsync(us =>
                    us.UserId == invitedUser.Id && us.SquadId == dto.SquadId))
                    return Json(new { success = false, message = "Пользователь уже в команде" });

                var invitation = new Invitation
                {
                    InvitedUserId = invitedUser.Id,
                    SquadId = dto.SquadId,
                    InviterUserId = currentUser.Id,
                    Status = InvitationStatus.Pending
                };

                _context.Invitations.Add(invitation);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Приглашение отправлено!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Ошибка: {ex.Message}" });
            }
        }
        public class CreateInvitationDto
        {
            public string InvitedUserName { get; set; }
            public Guid SquadId { get; set; }
        }

        // Просмотр своих приглашений
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var invitations = await _context.Invitations
                .Include(i => i.Squad)
                .Include(i => i.InviterUser)
                .Where(i => i.InvitedUserId == currentUser.Id && i.Status == InvitationStatus.Pending)
                .ToListAsync();

            return View(invitations);
        }

        // Обработка ответа
        [HttpPost]
        public async Task<IActionResult> Respond(Guid invitationId, bool accept)
        {
            var invitation = await _context.Invitations
                .Include(i => i.Squad)
                .FirstOrDefaultAsync(i => i.Id == invitationId);

            if (invitation == null) return NotFound();

            if (accept)
            {
                // Добавляем в команду
                _context.UserSquads.Add(new UserSquad
                {
                    UserId = invitation.InvitedUserId,
                    SquadId = invitation.SquadId,
                    JoinedAt = DateTime.UtcNow
                });
            }

            invitation.Status = accept ? InvitationStatus.Accepted : InvitationStatus.Rejected;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
