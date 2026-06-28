using System;
using System.Linq;
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
    public class ImplementedServicesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ImplementedServicesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _db.ImplementedServices.OrderBy(s => s.ServiceName).ToListAsync();
            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ImplementedService { CreationDate = DateTime.UtcNow, IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ImplementedService model)
        {
            if (model == null) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            model.CreationDate = DateTime.UtcNow;
            _db.ImplementedServices.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.ImplementedServices.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ImplementedService model)
        {
            if (model == null) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var existing = await _db.ImplementedServices.FindAsync(model.ServiceId);
            if (existing == null) return NotFound();

            existing.ServiceName = model.ServiceName;
            existing.ShortBrief = model.ShortBrief;
            existing.IconUrl = model.IconUrl;
            existing.ServiceUrl = model.ServiceUrl;
            existing.UpdatedUserId = model.UpdatedUserId;
            existing.UpdatedDate = DateTime.UtcNow;
            existing.IsActive = model.IsActive;

            _db.ImplementedServices.Update(existing);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.ImplementedServices.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.ImplementedServices.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.ImplementedServices.FindAsync(id);
            if (item == null) return NotFound();
            _db.ImplementedServices.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
