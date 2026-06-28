using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COADigitalServices.BLL;
using COADigitalServices.Data.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            var q = _db.Users.Include(u => u.Role).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                q = q.Where(u => u.Username.Contains(search) || (u.FirstName != null && u.FirstName.Contains(search)) || (u.LastName != null && u.LastName.Contains(search)));
                ViewData["Search"] = search;
            }

            var users = await q.OrderBy(u => u.Username).ToListAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Roles"] = new SelectList(_db.Roles.OrderBy(r => r.Name), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(string.Empty, "Username and password are required.");
                ViewData["Roles"] = new SelectList(_db.Roles.OrderBy(r => r.Name), "Id", "Name", model?.RoleId);
                return View(model);
            }

            if (await _db.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError(string.Empty, "Username already exists.");
                ViewData["Roles"] = new SelectList(_db.Roles.OrderBy(r => r.Name), "Id", "Name", model.RoleId);
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                PasswordHash = ComputeSha256Hash(model.Password),
                RoleId = model.RoleId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailAddress = model.EmailAddress,
                MobileNumber = model.MobileNumber
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
                RoleId = user.RoleId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailAddress = user.EmailAddress,
                MobileNumber = user.MobileNumber
            };
            ViewData["Roles"] = new SelectList(_db.Roles.OrderBy(r => r.Name), "Id", "Name", model.RoleId);
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
                ViewData["Roles"] = new SelectList(_db.Roles.OrderBy(r => r.Name), "Id", "Name", model.RoleId);
                return View(model);
            }

            if (await _db.Users.AnyAsync(u => u.Username == model.Username && u.Id != model.Id))
            {
                ModelState.AddModelError(string.Empty, "Another user with the same username exists.");
                ViewData["Roles"] = new SelectList(_db.Roles.OrderBy(r => r.Name), "Id", "Name", model.RoleId);
                return View(model);
            }

            user.Username = model.Username;
            user.RoleId = model.RoleId;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.EmailAddress = model.EmailAddress;
            user.MobileNumber = model.MobileNumber;
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
            var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
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
            [Required(ErrorMessage = "Username is required")]
            [MaxLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
            [Display(Name = "Username")]
            public string Username { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Role is required")]
            [Display(Name = "Role")]
            public int RoleId { get; set; }

            [Required(ErrorMessage = "First Name is required")]
            [MaxLength(100, ErrorMessage = "First Name cannot exceed 100 characters")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [MaxLength(100, ErrorMessage = "Last Name cannot exceed 100 characters")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Email Address is required")]
            [EmailAddress(ErrorMessage = "Email Address must be a valid email")]
            [MaxLength(200, ErrorMessage = "Email Address cannot exceed 200 characters")]
            [Display(Name = "Email Address")]
            public string EmailAddress { get; set; }

            [Required(ErrorMessage = "Mobile Number is required")]
            [Phone(ErrorMessage = "Mobile Number must be a valid phone number")]
            [MaxLength(20, ErrorMessage = "Mobile Number cannot exceed 20 characters")]
            [RegularExpression(@"^\d+$", ErrorMessage = "Mobile Number must contain only digits")]
            [Display(Name = "Mobile Number")]
            public string MobileNumber { get; set; }
        }

        public class EditUserModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Username is required")]
            [MaxLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
            [Display(Name = "Username")]
            public string Username { get; set; }

            [Display(Name = "Password (leave blank to keep current)")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Role is required")]
            [Display(Name = "Role")]
            public int RoleId { get; set; }

            [Required(ErrorMessage = "First Name is required")]
            [MaxLength(100, ErrorMessage = "First Name cannot exceed 100 characters")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [MaxLength(100, ErrorMessage = "Last Name cannot exceed 100 characters")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Email Address is required")]
            [EmailAddress(ErrorMessage = "Email Address must be a valid email")]
            [MaxLength(200, ErrorMessage = "Email Address cannot exceed 200 characters")]
            [Display(Name = "Email Address")]
            public string EmailAddress { get; set; }

            [Required(ErrorMessage = "Mobile Number is required")]
            [Phone(ErrorMessage = "Mobile Number must be a valid phone number")]
            [MaxLength(20, ErrorMessage = "Mobile Number cannot exceed 20 characters")]
            [RegularExpression(@"^\d+$", ErrorMessage = "Mobile Number must contain only digits")]
            [Display(Name = "Mobile Number")]
            public string MobileNumber { get; set; }
        }
    }
}
