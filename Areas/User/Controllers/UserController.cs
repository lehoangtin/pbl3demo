using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using System.Security.Claims; 
namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env; 
        private readonly UserManager<AppUser> _userManager;

        public UserController(AppDbContext context, IWebHostEnvironment env, UserManager<AppUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        public IActionResult Index()
{
    var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
    return RedirectToAction("Profile", "User", new { area = "User", id = userId });
}
        // 👤 Profile
        public IActionResult Profile(string id)
        {
            var user = _context.Users
                .Include(u => u.Questions)
                .Include(u => u.Answers)
                .FirstOrDefault(u => u.Id == id);

            if (user == null) return NotFound();

            return View(user);
        }
        [Authorize]
        public IActionResult Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = _context.Users.Find(userId);

            return View(user);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(AppUser model, IFormFile avatarFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = _context.Users.Find(userId);

            if (user == null) return NotFound();

            // update info
            user.FullName = model.FullName;
            user.Email = model.Email;

            // 🔥 upload avatar
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var ext = Path.GetExtension(avatarFile.FileName);
                var fileName = Guid.NewGuid() + ext;

                var path = Path.Combine(_env.WebRootPath, "images", fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await avatarFile.CopyToAsync(stream);

                user.Avatar = "/images/" + fileName;
            }

            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Profile", new { id = userId });
        }
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);

            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.OldPassword,
                model.NewPassword
            );

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            return RedirectToAction("Profile", new { id = user.Id });
        }
    }
}