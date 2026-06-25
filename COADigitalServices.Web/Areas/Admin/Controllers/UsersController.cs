using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COADigitalServices.BLL;
using COADigitalServices.Data.Models;

namespace COADigitalServices.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UsersController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string search)
        {
            var q = _db.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                q = q.Where(u => u.Username.Contains(search));
                ViewData["Search"] = search;
            }

            var users = await q.OrderBy(u => u.Username).ToListAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserModel model)
        {
            if (string.IsNullOrWhiteSpace(model?.Username) || string.IsNullOrWhiteSpace(model?.Password))
            {
                ModelState.AddModelError(string.Empty, "Username and password are required.");
                return View(model);
            }

            if (await _db.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError(string.Empty, "Username already exists.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                PasswordHash = ComputeSha256Hash(model.Password),
                Role = model.Role
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            var model = new EditUserModel
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserModel model)
        {
            if (model == null) return BadRequest();
            var user = await _db.Users.FindAsync(model.Id);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(model.Username))
            {
                ModelState.AddModelError(string.Empty, "Username is required.");
                return View(model);
            }

            if (await _db.Users.AnyAsync(u => u.Username == model.Username && u.Id != model.Id))
            {
                ModelState.AddModelError(string.Empty, "Another user with the same username exists.");
                return View(model);
            }

            user.Username = model.Username;
            user.Role = model.Role;
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.PasswordHash = ComputeSha256Hash(model.Password);
            }

            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private static string ComputeSha256Hash(string rawData)
        {
            if (rawData == null) return string.Empty;
            using var sha256Hash = SHA256.Create();
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var builder = new StringBuilder();
            foreach (var t in bytes) builder.Append(t.ToString("x2"));
            return builder.ToString();
        }

        public class CreateUserModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Role { get; set; }
        }

        public class EditUserModel
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Role { get; set; }
        }
    }
}
