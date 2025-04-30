using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkWise.Models;
using WorkWise.ViewModels;

namespace WorkWise.Controllers
{
    [Authorize]
    public class ProfilesController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ProfilesController(
            UserManager<Users> userManager,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _environment = environment;
        }

        [HttpGet("profile/{id?}")]
        public async Task<IActionResult> Index(string? id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var user = id == null ? currentUser :
                await _userManager.FindByIdAsync(id);

            if (user == null) return NotFound();

            var model = new ProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl,
                BirthDate = user.BirthDate,
                Bio = user.Bio,
                IsCurrentUser = user.Id == currentUser.Id
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new ProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl,
                BirthDate = user.BirthDate,
                Bio = user.Bio,
                IsCurrentUser = true
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                model.ProfilePictureUrl = (await _userManager.GetUserAsync(User))?.ProfilePictureUrl;
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Обработка фото
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploadsDir);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProfilePicture.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                {
                    var oldPath = Path.Combine(_environment.WebRootPath,
                        user.ProfilePictureUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                user.ProfilePictureUrl = $"/uploads/avatars/{fileName}";
            }

            user.FullName = model.FullName;
            if (model.BirthDate.HasValue)
            {
                user.BirthDate = DateTime.SpecifyKind(model.BirthDate.Value, DateTimeKind.Utc);
            }
            else
            {
                user.BirthDate = null;
            }
            user.Bio = model.Bio;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                model.ProfilePictureUrl = user.ProfilePictureUrl;
                return View(model);
            }

            return RedirectToAction("Index", new { id = user.Id });
        }
    }
}
