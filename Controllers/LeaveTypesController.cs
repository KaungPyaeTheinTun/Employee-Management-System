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

namespace EmployeeManagement.Controllers
{
    public class LeaveTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeaveTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LeaveTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.LeaveTypes.ToListAsync());
        }

        // GET: LeaveTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (leaveType == null)
            {
                return NotFound();
            }

            return View(leaveType);
        }

        // GET: LeaveTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LeaveTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Code,CreatedById,CreatedOn,ModifiedById,ModifiedOn")] LeaveType leaveType)
        {
            if (ModelState.IsValid)
            {
                var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                leaveType.CreatedById = UserId;
                leaveType.CreatedOn = DateTime.Now;
                _context.Add(leaveType);
                await _context.SaveChangesAsync(UserId);
                return RedirectToAction(nameof(Index));
            }
            return View(leaveType);
        }

        // GET: LeaveTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveType = await _context.LeaveTypes.FindAsync(id);
            if (leaveType == null)
            {
                return NotFound();
            }
            return View(leaveType);
        }

        // POST: LeaveTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Code,CreatedById,CreatedOn,ModifiedById,ModifiedOn")] LeaveType leaveType)
        {
            if (id != leaveType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    //Get Old Value
                    var oldLeaveValue = await _context.LeaveTypes.FindAsync(id);
                    leaveType.ModifiedById = UserId;
                    leaveType.ModifiedOn = DateTime.Now;
                    _context.Entry(oldLeaveValue).CurrentValues.SetValues(leaveType);
                    await _context.SaveChangesAsync(UserId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeaveTypeExists(leaveType.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(leaveType);
        }

        // GET: LeaveTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (leaveType == null)
            {
                return NotFound();
            }

            return View(leaveType);
        }

        // POST: LeaveTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var leaveType = await _context.LeaveTypes.FindAsync(id);
            if (leaveType != null)
            {
                _context.LeaveTypes.Remove(leaveType);
            }
            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _context.SaveChangesAsync(UserId);
            return RedirectToAction(nameof(Index));
        }

        private bool LeaveTypeExists(int id)
        {
            return _context.LeaveTypes.Any(e => e.Id == id);
        }
    }
}
