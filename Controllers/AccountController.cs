using Microsoft.AspNetCore.Mvc;
using UserManagementApp.Models;
using UserManagementApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace UserManagementApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDBContext _context;

        public AccountController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);


            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                RegisteredAt = DateTime.UtcNow,
                LastLoginAt = null
            };

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Email is already registered.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (!ModelState.IsValid)
                return View();


            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View();
            }

            if (user.IsBlocked)
            {
                ModelState.AddModelError("", "Your account is blocked.");
                return View();
            }

            user.LastLoginAt = DateTime.UtcNow;
            _context.Update(user);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.GivenName, user.Name),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        public async Task<IActionResult> UserList()
        {
            if (User?.Identity?.Name == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            if (user == null || user.IsBlocked)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login", "Account");
            }

            var users = await _context.Users
                .OrderByDescending(u => u.LastLoginAt ?? DateTime.MinValue)
                .ToListAsync();

            return View(users);
        }

        private int? GetCurrentUserId()
        {
            if (User?.Identity?.Name == null)
                return null;

            var user = _context.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            return user?.Id ?? null;
        }

        [HttpPost]
        public async Task<IActionResult> BlockUsers([FromForm] List<int> userIds)
        {
            if (userIds == null || !userIds.Any())
            {
                TempData["Error"] = "No users selected for blocking.";
                return RedirectToAction("UserList");
            }

            var currentUserId = GetCurrentUserId();
            bool selfBlocked = userIds.Contains(currentUserId ?? -1);

            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            foreach (var user in users)
            {
                user.IsBlocked = true;
            }

            await _context.SaveChangesAsync();

            if (selfBlocked)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login", "Account");
            }

            TempData["Message"] = $"{users.Count} user(s) have been blocked successfully.";
            return RedirectToAction("UserList");
        }

        [HttpPost]
        public async Task<IActionResult> UnblockUsers([FromForm] List<int> userIds)
        {
            if (userIds == null || !userIds.Any())
            {
                TempData["Error"] = "No users selected for unblocking.";
                return RedirectToAction("UserList");
            }

            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            foreach (var user in users)
            {
                user.IsBlocked = false;
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = $"{users.Count} user(s) have been unblocked successfully.";
            return RedirectToAction("UserList");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUsers([FromForm] List<int> userIds)
        {
            if (userIds == null || !userIds.Any())
            {
                TempData["Error"] = "No users selected for deletion.";
                return RedirectToAction("UserList");
            }

            var currentUserId = GetCurrentUserId();
            bool selfDeleted = userIds.Contains(currentUserId ?? -1);

            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            _context.Users.RemoveRange(users);
            await _context.SaveChangesAsync();

            if (selfDeleted)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login", "Account");
            }

            TempData["Message"] = $"{users.Count} user(s) have been deleted successfully.";
            return RedirectToAction("UserList");
        }
    }
}