using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
                .Include(s => s.Goals)
                    .ThenInclude(g => g.Performer)
                .FirstOrDefaultAsync(s => s.Id == id);


            if (squad == null)
                return NotFound();

            return View(squad);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember([FromBody] RemoveMemberModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var squad = await _context.Squads.FindAsync(model.squadId);

            if (squad == null)
                return NotFound("Команда не найдена");

            if (squad.LeaderID != currentUser.Id)
                return Forbid();

            if (model.userId == squad.LeaderID)
                return BadRequest("Нельзя удалить лидера команды");

            var userSquad = await _context.UserSquads
                .FirstOrDefaultAsync(us => us.UserId == model.userId && us.SquadId == model.squadId);

            if (userSquad == null)
                return NotFound("Участник не найден");

            _context.UserSquads.Remove(userSquad);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        public class RemoveMemberModel
        {
            public Guid squadId { get; set; }
            public string userId { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> CreateGoal(Guid squadId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var squad = await _context.Squads
                .Include(s => s.UserSquads)
                    .ThenInclude(us => us.User)
                .FirstOrDefaultAsync(s => s.Id == squadId);

            if (squad == null)
                return NotFound();

            // Проверка, что текущий пользователь - лидер команды
            if (squad.LeaderID != currentUserId)
            {
                return Forbid(); // Или RedirectToAction с сообщением об ошибке
            }

            var model = new GoalCreateViewModel
            {
                SquadId = squadId,
                AvailablePerformers = squad.UserSquads
                    .Select(us => new SelectListItem
                    {
                        Value = us.User.Id,
                        Text = us.User.UserName
                    }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGoal(GoalCreateViewModel model)
        {
            var currentUserId = _userManager.GetUserId(User);
            var squad = await _context.Squads.FindAsync(model.SquadId);

            // Проверка перед обработкой формы
            if (squad == null)
                return NotFound();

            if (squad.LeaderID != currentUserId)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                squad = await _context.Squads
                    .Include(s => s.UserSquads)
                        .ThenInclude(us => us.User)
                    .FirstOrDefaultAsync(s => s.Id == model.SquadId);

                model.AvailablePerformers = squad.UserSquads
                    .Select(us => new SelectListItem
                    {
                        Value = us.User.Id,
                        Text = us.User.UserName
                    }).ToList();

                return View(model);
            }

            var goal = new Goals
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Desc = model.Desc,
                Importancy = model.Importancy,
                FinishTime = model.FinishTime.ToUniversalTime(),    
                CreatedAt = DateTime.UtcNow,
                State = false,
                SquadId = model.SquadId,
                PerformerID = string.IsNullOrEmpty(model.PerformerID) ? null : model.PerformerID
            };

            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = model.SquadId });
        }
        [HttpGet]
        public async Task<IActionResult> EditGoal(Guid id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var goal = await _context.Goals
                .Include(g => g.Squad)
                .Include(g => g.Performer)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (goal == null || goal.Squad.LeaderID != currentUserId)
                return Forbid();

            var squad = await _context.Squads
                .Include(s => s.UserSquads)
                    .ThenInclude(us => us.User)
                .FirstOrDefaultAsync(s => s.Id == goal.SquadId);

            var model = new GoalEditViewModel
            {
                Id = goal.Id,
                Name = goal.Name,
                Desc = goal.Desc,
                Importancy = goal.Importancy,
                FinishTime = goal.FinishTime.ToUniversalTime(),
                PerformerID = goal.PerformerID,
                SquadId = goal.SquadId,
                IsCompleted = goal.State,
                AvailablePerformers = squad.UserSquads
                    .Select(us => new SelectListItem
                    {
                        Value = us.User.Id,
                        Text = us.User.UserName
                    }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGoal(GoalEditViewModel model)
        {
            var currentUserId = _userManager.GetUserId(User);
            var goal = await _context.Goals
                .Include(g => g.Squad)
                .FirstOrDefaultAsync(g => g.Id == model.Id);

            if (goal == null || goal.Squad.LeaderID != currentUserId)
                return Forbid();

            if (ModelState.IsValid)
            {
                goal.Name = model.Name;
                goal.Desc = model.Desc;
                goal.Importancy = model.Importancy;
                goal.FinishTime = model.FinishTime.ToUniversalTime();
                goal.PerformerID = model.PerformerID;
                goal.State = model.IsCompleted;

                _context.Update(goal);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = model.SquadId });
            }

            var squad = await _context.Squads
                .Include(s => s.UserSquads)
                    .ThenInclude(us => us.User)
                .FirstOrDefaultAsync(s => s.Id == model.SquadId);

            model.AvailablePerformers = squad.UserSquads
                .Select(us => new SelectListItem
                {
                    Value = us.User.Id,
                    Text = us.User.UserName
                }).ToList();

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGoal(Guid id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var goal = await _context.Goals
                .Include(g => g.Squad)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (goal == null || goal.Squad.LeaderID != currentUserId)
                return Forbid();

            _context.Goals.Remove(goal);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = goal.SquadId });
        }
        [HttpGet("MyTasks")]
        public async Task<IActionResult> MyTasks(string filter = "active")
        {
            var currentUserId = _userManager.GetUserId(User);

            var query = _context.Goals
                .Include(g => g.Squad)
                .Include(g => g.Performer)
                .Where(g => g.PerformerID == currentUserId);

            query = filter switch
            {
                "completed" => query.Where(g => g.State),
                "overdue" => query.Where(g => !g.State && g.FinishTime < DateTime.UtcNow),
                _ => query.Where(g => !g.State) // active по умолчанию
            };

            var tasks = await query
                .OrderByDescending(g => g.Importancy)
                .ThenBy(g => g.FinishTime)
                .ToListAsync();

            ViewBag.CurrentFilter = filter;
            return View(tasks);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsCompleted(Guid taskId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var task = await _context.Goals
                .FirstOrDefaultAsync(g => g.Id == taskId && g.PerformerID == currentUserId);

            if (task == null)
                return NotFound();

            task.State = true;
            _context.Update(task);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyTasks");
        }
        [HttpGet]
        public async Task<IActionResult> AddFeedback(Guid goalId)
        {
            var goal = await _context.Goals
                .Include(g => g.Squad)
                .FirstOrDefaultAsync(g => g.Id == goalId);

            if (goal == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var isLeader = goal.Squad?.LeaderID == currentUserId;
            var isPerformer = goal.PerformerID == currentUserId;
            if (!isLeader && !isPerformer)
                return Forbid();

            bool canAddFeedback = goal.PerformerID == currentUserId ||
                                goal.Squad.LeaderID == currentUserId;

            if (!canAddFeedback)
                return Forbid();

            var model = new GoalFeedbackViewModel
            {
                GoalId = goal.Id,
                GoalName = goal.Name,
                IsLeader = goal.Squad.LeaderID == currentUserId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFeedback(GoalFeedbackViewModel model)
        {
            var goal = await _context.Goals
                .Include(g => g.Squad)
                .FirstOrDefaultAsync(g => g.Id == model.GoalId);

            if (goal == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var isLeader = goal.Squad?.LeaderID == currentUserId;
            var isPerformer = goal.PerformerID == currentUserId;
            if (!isLeader && !isPerformer)
                return Forbid();
            bool canAddFeedback = goal.PerformerID == currentUserId ||
                                goal.Squad.LeaderID == currentUserId;

            if (!canAddFeedback)
                return Forbid();

            if (ModelState.IsValid)
            {
                var feedback = new GoalFeedback
                {
                    Comment = model.Comment,
                    CreatedAt = DateTime.UtcNow,
                    GoalId = model.GoalId,
                    AuthorId = currentUserId,
                    IsLeaderComment = model.IsLeader // Добавляем метку комментария лидера
                };

                _context.GoalFeedbacks.Add(feedback);

                // Если это исполнитель и отметил как выполненную
                if (!model.IsLeader && model.MarkAsCompleted)
                {
                    goal.State = true;
                    _context.Goals.Update(goal);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("TaskDetails", new { id = model.GoalId });
            }

            model.GoalName = goal.Name;
            model.IsLeader = goal.Squad.LeaderID == currentUserId;
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFeedback(Guid id)
        {
            var feedback = await _context.GoalFeedbacks
                .Include(f => f.Author)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (feedback == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (feedback.AuthorId != currentUserId)
                return Forbid();

            _context.GoalFeedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            return RedirectToAction("TaskDetails", new { id = feedback.GoalId });
        }
        [HttpGet("TaskDetails/{id}")]
        public async Task<IActionResult> TaskDetails(Guid id)
        {
            var task = await _context.Goals
                .Include(g => g.Squad)
                    .ThenInclude(s => s.Leader)
                .Include(g => g.Performer)
                .Include(g => g.Feedbacks)
                    .ThenInclude(f => f.Author)
                .Include(g => g.Attachments)
                    .ThenInclude(a => a.UploadedBy)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (task == null)
                return NotFound();
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = _userManager.GetUserId(User);
            var hasAccess = task.Squad.LeaderID == currentUserId ||
                           task.PerformerID == currentUserId ||
                           await _context.UserSquads
                               .AnyAsync(us => us.UserId == currentUserId && us.SquadId == task.SquadId);

            if (!hasAccess)
                return Forbid();
            return View(task);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteGoal(Guid id)
        {
            var goal = await _context.Goals
                .Include(g => g.Squad)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (goal == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (goal.Squad.LeaderID != currentUserId)
                return Forbid();

            goal.State = true;
            _context.Goals.Update(goal);
            await _context.SaveChangesAsync();

            return RedirectToAction("TaskDetails", new { id = goal.Id });
        }
    }
}

