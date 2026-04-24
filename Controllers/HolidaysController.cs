using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Data;
using EmployeesManagement.Models;
using System.Security.Claims;
using EmployeeManagement.ViewModels;
using EmployeesManagement.Services;

namespace EmployeeManagement.Controllers
{
    public class HolidaysController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HolidaysController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Holidays
        public async Task<IActionResult> Index(HolidayViewModel vm)
        {
            var query = _context.Holidays
                        .Include(x => x.CreatedBy)
                        .AsQueryable();

            if (!string.IsNullOrWhiteSpace(vm.SearchTerm))
            {
                var searchTerm = vm.SearchTerm.Trim();

                DateTime searchDate;
                bool isDate = DateTime.TryParse(searchTerm, out searchDate);

                query = query.Where(x =>
                    (x.Title != null && x.Title.Contains(searchTerm)) ||
                    (x.Description != null && x.Description.Contains(searchTerm)) ||

                    (isDate && x.StartDate.Date <= searchDate.Date && x.EndDate.Date >= searchDate.Date)
                );
            }

            // Assign result to ViewModel
            vm.Holidays = await query.ToListAsync();

            return View(vm);
        }

        // GET: Holidays/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var holiday = await _context.Holidays
                .Include(x => x.CreatedBy)
                .Include(x => x.ModifiedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (holiday == null)
            {
                return NotFound();
            }

            return View(holiday);
        }

        // GET: Holidays/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Holidays/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Holiday holiday)
        {
            var UserId = User.GetUserId();
            holiday.CreatedById = UserId;
            holiday.CreatedOn = DateTime.Now;

            _context.Add(holiday);
            await _context.SaveChangesAsync(UserId);
            TempData["SuccessMessage"] = "Holiday created successfully.";

            return RedirectToAction(nameof(Index));

            return View(holiday);
        }

        // GET: Holidays/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var holiday = await _context.Holidays
                        .Include(x => x.CreatedBy)
                        .Include(x => x.ModifiedBy)
                        .FirstOrDefaultAsync(x => x.Id == id);
            if (holiday == null)
            {
                return NotFound();
            }
            return View(holiday);
        }

        // POST: Holidays/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Holiday holiday)
        {
            if (id != holiday.Id)
            {
                return NotFound();
            }

            var UserId = User.GetUserId();
            holiday.ModifiedById = UserId;
            holiday.ModifiedOn = DateTime.Now;

            try
            {
                _context.Update(holiday);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HolidayExists(holiday.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));

            return View(holiday);
        }

        // GET: Holidays/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var holiday = await _context.Holidays
                .Include(h => h.CreatedBy)
                .Include(h => h.ModifiedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (holiday == null)
            {
                return NotFound();
            }

            return View(holiday);
        }

        // POST: Holidays/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var holiday = await _context.Holidays.FindAsync(id);
            if (holiday != null)
            {
                _context.Holidays.Remove(holiday);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Holiday Delete successfully.";

            return RedirectToAction(nameof(Index));
        }

        private bool HolidayExists(int id)
        {
            return _context.Holidays.Any(e => e.Id == id);
        }
    }
}
