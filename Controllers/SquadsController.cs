using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkWise.Data;
using WorkWise.Models;
using WorkWise.ViewModels;

namespace WorkWise.Controllers
{
    [Authorize]
    public class SquadsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public SquadsController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Squad (список всех команд)
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            var squads = await _context.Squads
                .Where(s => s.LeaderID == currentUserId ||
                           s.UserSquads.Any(us => us.UserId == currentUserId))
                .Include(s => s.Leader)
                .ToListAsync();

            // Преобразуем в ViewModel
            var viewModel = squads.Select(s => new SquadViewModel
            {
                Id = s.Id,
                Name = s.Name,
                LeaderName = s.Leader.UserName,
                IsCurrentUserLeader = s.LeaderID == currentUserId
            }).ToList();

            return View(viewModel);
        }

        // GET: /Squad/Create (форма создания)
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Squad/Create (обработка формы)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] SquadsCreateModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);

                var squad = new Squads
                {
                    Name = model.Name,
                    LeaderID = currentUser.Id,
                    Leader = currentUser
                };

                _context.Add(squad);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /Squad/Details/5 (просмотр команды)
        public async Task<IActionResult> Details(Guid id)
        {
            var squad = await _context.Squads
                .Include(s => s.Leader)
                .Include(s => s.UserSquads)
                    .ThenInclude(us => us.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (squad == null)
                return NotFound();

            return View(squad);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(Guid squadId, string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var squad = await _context.Squads.FindAsync(squadId);

            if (squad.LeaderID != currentUser.Id)
                return Forbid();

            if (userId == squad.LeaderID)
                return BadRequest("Нельзя удалить лидера команды");

            var userSquad = await _context.UserSquads
                .FirstOrDefaultAsync(us => us.UserId == userId && us.SquadId == squadId);

            if (userSquad == null)
                return NotFound();

            _context.UserSquads.Remove(userSquad);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}

