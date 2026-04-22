using EmployeeManagement.Data;
using EmployeesManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EmployeesManagement.Controllers
{
    public class WorkFlowUserGroupMembersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkFlowUserGroupMembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: WorkFlowUserGroupMenbers
        public async Task<IActionResult> Index(int? id)
        {
            var members = await _context
                        .WorkFlowUserGroupMembers
                        .Include(w => w.Approver)
                        .Include(w => w.Sender)
                        .Include(w => w.WorkFlowUserGroup)
                        .ToListAsync();
            return View(members);
        }

        // GET: WorkFlowUserGroupMenbers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workFlowUserGroupMenber = await _context.WorkFlowUserGroupMembers
                .Include(w => w.Approver)
                .Include(w => w.Sender)
                .Include(w => w.WorkFlowUserGroup)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workFlowUserGroupMenber == null)
            {
                return NotFound();
            }

            return View(workFlowUserGroupMenber);
        }

        // GET: WorkFlowUserGroupMenbers/Create
        public IActionResult Create()
        {
            ViewData["ApproverId"] = new SelectList(_context.Users, "Id", "FullName");
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "FullName");
            ViewData["WorkFlowUserGroupId"] = new SelectList(_context.WorkFlowUserGroups, "Id", "Description");
            return View();
        }

        // POST: WorkFlowUserGroupMenbers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkFlowUserGroupMember workFlowUserGroupMenber)
        {
                var Userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _context.Add(workFlowUserGroupMenber);
                await _context.SaveChangesAsync(Userid);
                TempData["SuccessMessage"] = "WorkFlowUserGroupMember created successfully.";

                return RedirectToAction(nameof(Index));

            ViewData["ApproverId"] = new SelectList(_context.Users, "Id", "FullName", workFlowUserGroupMenber.ApproverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "FullName", workFlowUserGroupMenber.SenderId);
            ViewData["WorkFlowUserGroupId"] = new SelectList(_context.WorkFlowUserGroups, "Id", "Description", workFlowUserGroupMenber.WorkFlowUserGroupId);
            return View(workFlowUserGroupMenber);
        }

        // GET: WorkFlowUserGroupMenbers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workFlowUserGroupMenber = await _context.WorkFlowUserGroupMembers.FindAsync(id);
            if (workFlowUserGroupMenber == null)
            {
                return NotFound();
            }
            ViewData["ApproverId"] = new SelectList(_context.Users, "Id", "Id", workFlowUserGroupMenber.ApproverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", workFlowUserGroupMenber.SenderId);
            ViewData["WorkFlowUserGroupId"] = new SelectList(_context.WorkFlowUserGroups, "Id", "Id", workFlowUserGroupMenber.WorkFlowUserGroupId);
            return View(workFlowUserGroupMenber);
        }
        // POST: WorkFlowUserGroupMenbers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,WorkFlowUserGroupId,SenderId,ApproverId,SequenceNo")] WorkFlowUserGroupMember workFlowUserGroupMenber)
        {
            if (id != workFlowUserGroupMenber.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var Userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    _context.Update(workFlowUserGroupMenber);
                    await _context.SaveChangesAsync(Userid);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkFlowUserGroupMenberExists(workFlowUserGroupMenber.Id))
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
            ViewData["ApproverId"] = new SelectList(_context.Users, "Id", "Id", workFlowUserGroupMenber.ApproverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", workFlowUserGroupMenber.SenderId);
            ViewData["WorkFlowUserGroupId"] = new SelectList(_context.WorkFlowUserGroups, "Id", "Id", workFlowUserGroupMenber.WorkFlowUserGroupId);
            return View(workFlowUserGroupMenber);
        }

        // GET: WorkFlowUserGroupMenbers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workFlowUserGroupMenber = await _context.WorkFlowUserGroupMembers
                .Include(w => w.Approver)
                .Include(w => w.Sender)
                .Include(w => w.WorkFlowUserGroup)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workFlowUserGroupMenber == null)
            {
                return NotFound();
            }

            return View(workFlowUserGroupMenber);
        }

        // POST: WorkFlowUserGroupMenbers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workFlowUserGroupMenber = await _context.WorkFlowUserGroupMembers.FindAsync(id);
            if (workFlowUserGroupMenber != null)
            {
                _context.WorkFlowUserGroupMembers.Remove(workFlowUserGroupMenber);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkFlowUserGroupMenberExists(int id)
        {
            return _context.WorkFlowUserGroupMembers.Any(e => e.Id == id);
        }
    }
}