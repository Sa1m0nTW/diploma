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
                .Include(s => s.UserSquads)
                .ToListAsync();

            var userRoles = await _context.UserSquads
            .Where(us => us.UserId == currentUserId)
            .ToDictionaryAsync(us => us.SquadId, us => us.Role);

            var viewModel = squads.Select(s => new SquadViewModel
            {
                Id = s.Id,
                Name = s.Name,
                LeaderName = s.Leader.UserName,
                IsCurrentUserLeader = s.LeaderID == currentUserId,
                UserRole = s.LeaderID == currentUserId
                    ? "Лидер"
                    : userRoles.TryGetValue(s.Id, out var role) ? role : null
            }).ToList();

            return View(viewModel);
        }
        public IActionResult Create()
        {
            return View();
        }

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
        public async Task<IActionResult> Details(Guid id)
        {
            var squad = await _context.Squads
                .Include(s => s.Leader)
                .Include(s => s.UserSquads)
                    .ThenInclude(us => us.User)
                .Include(s => s.Goals)
                    .ThenInclude(g => g.Performers)
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
            var tasksWithUser = await _context.Goals
                .Include(g => g.Performers)
                     .Where(g => g.SquadId == squad.Id && g.Performers.Any(p => p.Id == model.userId))
                .ToListAsync();

            // Удаляем пользователя из списка исполнителей задач
            foreach (var task in tasksWithUser)
            {
                var performerToRemove = task.Performers.FirstOrDefault(p => p.Id == userSquad.UserId);
                task.Performers.Remove(performerToRemove);
            }

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

            if (squad.LeaderID != currentUserId)
            {
                return Forbid();
            }

            var model = new GoalCreateViewModel
            {
                SquadId = squadId,
                AvailablePerformers = squad.UserSquads.Select(us => new PerformerViewModel
                {
                    Id = us.User.Id,
                    Name = us.User.UserName,
                    IsSelected = false
                }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGoal(GoalCreateViewModel model)
        {
            var currentUserId = _userManager.GetUserId(User);
            var squad = await _context.Squads
                .Include(s => s.UserSquads)
                    .ThenInclude(us => us.User)
                .FirstOrDefaultAsync(s => s.Id == model.SquadId);

            if (squad == null)
                return NotFound();

            if (squad.LeaderID != currentUserId)
                return Forbid();

            if (!ModelState.IsValid)
            {
                // При ошибке валидации перезаполняем список исполнителей
                model.AvailablePerformers = squad.UserSquads
                    .Select(us => new PerformerViewModel
                    {
                        Id = us.User.Id,
                        Name = us.User.UserName,
                        IsSelected = model.AvailablePerformers?
                            .FirstOrDefault(p => p.Id == us.User.Id)?.IsSelected ?? false
                    })
                    .ToList();

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
            };

            // Находим выбранных исполнителей
            var selectedPerformerIds = model.AvailablePerformers
                .Where(p => p.IsSelected)
                .Select(p => p.Id)
                .ToList();

            var performers = await _context.Users
                .Where(u => selectedPerformerIds.Contains(u.Id))
                .ToListAsync();

            // Добавляем исполнителей к задаче
            goal.Performers = performers;

            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = model.SquadId });
        }
        [HttpGet]
        public async Task<IActionResult> EditGoal(Guid id)
        {
            var currentUserId = _userManager.GetUserId(User);

            // Загружаем задачу с связанными данными
            var goal = await _context.Goals
                .Include(g => g.Squad)
                .Include(g => g.Performers)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (goal == null || goal.Squad.LeaderID != currentUserId)
                return Forbid();

            // Загружаем всех возможных исполнителей (участников команды)
            var squadMembers = await _context.UserSquads
                .Where(us => us.SquadId == goal.SquadId)
                .Include(us => us.User)
                .ToListAsync();

            // Создаем модель для представления
            var model = new GoalEditViewModel
            {
                Id = goal.Id,
                Name = goal.Name,
                Desc = goal.Desc,
                Importancy = goal.Importancy,
                FinishTime = goal.FinishTime.ToUniversalTime(),
                SquadId = goal.SquadId,
                IsCompleted = goal.State,
                AvailablePerformers = squadMembers.Select(us => new PerformerViewModel
                {
                    Id = us.User.Id,
                    Name = us.User.UserName,
                    IsSelected = goal.Performers.Any(p => p.Id == us.User.Id)
                }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGoal(GoalEditViewModel model)
        {
            var currentUserId = _userManager.GetUserId(User);

            // Загружаем задачу со всеми необходимыми данными
            var goal = await _context.Goals
                .Include(g => g.Squad)
                .Include(g => g.Performers)
                .FirstOrDefaultAsync(g => g.Id == model.Id);

            if (goal == null || goal.Squad.LeaderID != currentUserId)
                return Forbid();

            if (!ModelState.IsValid)
            {
                // Перезагружаем список исполнителей при ошибке валидации
                var squadMembers = await _context.UserSquads
                    .Where(us => us.SquadId == goal.SquadId)
                    .Include(us => us.User)
                    .ToListAsync();

                model.AvailablePerformers = squadMembers.Select(us => new PerformerViewModel
                {
                    Id = us.User.Id,
                    Name = us.User.UserName,
                    IsSelected = model.AvailablePerformers?.Any(p => p.Id == us.User.Id && p.IsSelected) ?? false
                }).ToList();

                return View(model);
            }

            // Обновляем основные свойства задачи
            goal.Name = model.Name;
            goal.Desc = model.Desc;
            goal.Importancy = model.Importancy;
            goal.FinishTime = model.FinishTime.ToUniversalTime();
            goal.State = model.IsCompleted;

            // Обрабатываем изменения исполнителей
            var selectedPerformerIds = model.AvailablePerformers
                .Where(p => p.IsSelected)
                .Select(p => p.Id)
                .ToList();

            // Загружаем всех пользователей, которые могут быть исполнителями
            var allPossiblePerformers = await _context.UserSquads
                .Where(us => us.SquadId == goal.SquadId)
                .Select(us => us.User)
                .ToListAsync();

            // Очищаем текущих исполнителей и добавляем новых
            goal.Performers.Clear();

            foreach (var performerId in selectedPerformerIds)
            {
                var performer = allPossiblePerformers.FirstOrDefault(p => p.Id == performerId);
                if (performer != null)
                {
                    goal.Performers.Add(performer);
                }
            }

            try
            {
                _context.Update(goal);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = goal.SquadId });
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Не удалось сохранить изменения: " + ex.Message);

                var squadMembers = await _context.UserSquads
                    .Where(us => us.SquadId == goal.SquadId)
                    .Include(us => us.User)
                    .ToListAsync();

                model.AvailablePerformers = squadMembers.Select(us => new PerformerViewModel
                {
                    Id = us.User.Id,
                    Name = us.User.UserName,
                    IsSelected = selectedPerformerIds.Contains(us.User.Id)
                }).ToList();

                return View(model);
            }
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
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = _userManager.GetUserId(User);

            var query = _context.Goals
                .Include(g => g.Squad)
                .Include(g => g.Performers)
                .Where(g => g.Performers.Any(p => p.Id == currentUser.Id));

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
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = _userManager.GetUserId(User);
            var task = await _context.Goals
                .FirstOrDefaultAsync(g => g.Id == taskId && g.Performers.Any(p => p.Id == currentUser.Id));

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
            var currentUser = await _userManager.GetUserAsync(User);
            var goal = await _context.Goals
                .Include(g => g.Squad)
                .Include(g => g.Performers)
                .FirstOrDefaultAsync(g => g.Id == goalId);

            if (goal == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var isLeader = goal.Squad?.LeaderID == currentUserId;
            var isPerformer = goal.Performers.Any(p => p.Id == currentUserId);
            if (!isLeader && !isPerformer)
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
                .Include(g => g.Performers)
                .FirstOrDefaultAsync(g => g.Id == model.GoalId);

            if (goal == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = _userManager.GetUserId(User);
            var isLeader = goal.Squad?.LeaderID == currentUserId;
            var isPerformer = goal.Performers.Any(p => p.Id == currentUserId);
            if (!isLeader && !isPerformer)
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
                    .ThenInclude(s => s.UserSquads)
                        .ThenInclude(us => us.User)
                    .Include(g => g.Performers)
                .Include(g => g.Squad.Leader)
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
                           task.Performers.Any(p => p.Id == currentUser.Id) ||
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
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> TakeTask(Guid goalId)
        {
            var goal = await _context.Goals
                .Include(g => g.Squad)
                    .ThenInclude(s => s.UserSquads)
                        .ThenInclude(us => us.User)
                    .Include(g => g.Performers)
                .FirstOrDefaultAsync(g => g.Id == goalId);

            if (goal == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            if (!goal.Performers.Any(p => p.Id == currentUser.Id) &&
                goal.Squad.Leader?.UserName != User.Identity?.Name &&
                goal.Squad.UserSquads.Any(us => us.User.UserName == User.Identity?.Name))
            {
                goal.Performers.Add(currentUser);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Вы взяли задачу!";
            }
            else
            {
                TempData["ErrorMessage"] = "Не удалось взять задачу.";
            }

            return RedirectToAction("TaskDetails", new { id = goalId });
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole([FromForm] ChangeRoleViewModel model)
        {
            var squad = await _context.Squads
                .Include(s => s.Leader)
                .Include(s => s.UserSquads)
                    .ThenInclude(us => us.User)
                .FirstOrDefaultAsync(s => s.Id == model.SquadId);

            if (squad == null)
                return NotFound();

            // Проверяем, что текущий пользователь - лидер команды
            if (squad.Leader.UserName != User.Identity.Name)
                return Forbid();

            var userSquad = squad.UserSquads.FirstOrDefault(us => us.UserId == model.UserId);
            if (userSquad == null)
                return NotFound();

            userSquad.Role = model.NewRole;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Роль участника {userSquad.User.UserName} изменена на {model.NewRole}";
            return RedirectToAction("Details", new { id = model.SquadId });
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var squad = await _context.Squads
                .Include(s => s.Leader)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (squad == null)
            {
                return NotFound();
            }

            // Проверяем, что текущий пользователь - лидер команды
            if (squad.Leader.UserName != User.Identity.Name)
            {
                return Forbid();
            }

            // Удаляем все связанные данные (каскадное удаление)
            var goals = await _context.Goals.Where(g => g.SquadId == id).ToListAsync();
            var userSquads = await _context.UserSquads.Where(us => us.SquadId == id).ToListAsync();
            var invitations = await _context.Invitations.Where(i => i.SquadId == id).ToListAsync();

            _context.Goals.RemoveRange(goals);
            _context.UserSquads.RemoveRange(userSquads);
            _context.Invitations.RemoveRange(invitations);
            _context.Squads.Remove(squad);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Команда успешно удалена";
            return RedirectToAction("Index");
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveTeam(Guid squadId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userSquad = await _context.UserSquads
                .Include(us => us.Squad)
                .FirstOrDefaultAsync(us => us.SquadId == squadId && us.UserId == currentUser.Id);

            if (userSquad == null)
            {
                return NotFound();
            }

            // Проверяем, что пользователь не лидер команды
            if (userSquad.Squad.LeaderID == currentUser.Id)
            {
                TempData["ErrorMessage"] = "Лидер не может покинуть команду. Сначала передайте лидерство.";
                return RedirectToAction("Details", new { id = squadId });
            }
            var tasksWithUser = await _context.Goals
                .Include(g => g.Performers)
                     .Where(g => g.SquadId == squadId && g.Performers.Any(p => p.Id == currentUser.Id))
                .ToListAsync();

            // Удаляем пользователя из списка исполнителей задач
            foreach (var task in tasksWithUser)
            {
                var performerToRemove = task.Performers.FirstOrDefault(p => p.Id == currentUser.Id);
                task.Performers.Remove(performerToRemove);
            }

            _context.UserSquads.Remove(userSquad);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Вы успешно покинули команду";
            return RedirectToAction("Index", "Squads");
        }
        [HttpGet]
        public async Task<IActionResult> GetChatMessages(Guid squadId)
        {
            var messages = await _context.SquadChatMes
                .Where(m => m.SquadId == squadId)
                .OrderBy(m => m.CreatedAt)
                .Take(50)
                .Include(m => m.Author)
                .Select(m => new
                {
                    id = m.Id, // Добавляем ID сообщения
                    author = m.Author.UserName, // Используем UserName для проверки
                    text = m.Content,
                    date = m.CreatedAt.ToLocalTime().ToString("g"),
                    avatar = m.Author.ProfilePictureUrl
                })
                .ToListAsync();

            return Json(new { success = true, messages });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendChatMessage(Guid squadId, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                    return BadRequest("Сообщение не может быть пустым");

                var currentUser = await _userManager.GetUserAsync(User);

                var chatMessage = new SquadChatMes
                {
                    Id = Guid.NewGuid(),
                    Content = message,
                    CreatedAt = DateTime.UtcNow,
                    AuthorId = currentUser.Id,
                    SquadId = squadId
                };

                _context.SquadChatMes.Add(chatMessage);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var message = await _context.SquadChatMes
                .Include(m => m.Squad)
                .Include(m => m.Author)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
                return NotFound("Сообщение не найдено");

            // Проверяем права: автор или лидер команды
            bool isAuthor = message.AuthorId == currentUser.Id;
            bool isLeader = message.Squad.LeaderID == currentUser.Id;

            if (!isAuthor && !isLeader)
                return Forbid();

            _context.SquadChatMes.Remove(message);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}

