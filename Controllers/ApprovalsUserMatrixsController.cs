using EmployeeManagement.Data;
using EmployeesManagement.Models;
using EmployeesManagement.Services;
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
    public class ApprovalsUserMatrixsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApprovalsUserMatrixsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ApprovalsUserMetrices
        public async Task<IActionResult> Index()
        {
            var matrix = await _context.ApprovalsUserMatrixs
                .Include(a => a.DocumentType)
                .Include(a => a.User)
                .Include(a => a.CreatedBy)
                .Include(a => a.WorkFlowUserGroup)
                .ToListAsync();
            return View(matrix);
        }

        // GET: ApprovalsUserMetrices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var approvalsUserMatrix = await _context.ApprovalsUserMatrixs
                .Include(a => a.DocumentType)
                .Include(a => a.User)
                .Include(a => a.WorkFlowUserGroup)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (approvalsUserMatrix == null)
            {
                return NotFound();
            }

            return View(approvalsUserMatrix);
        }

        // GET: ApprovalsUserMetrices/Create
        public IActionResult Create()
        {
            ViewData["DocumentTypeId"] = new SelectList(_context.SystemCodeDetails.Include(x => x.SystemCode).Where(y => y.SystemCode.Code == "DocumentTypes"), "Id", "Description");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName");
            ViewData["WorkFlowUserGroupId"] = new SelectList(_context.WorkFlowUserGroups, "Id", "Description");
            return View();
        }

        // POST: ApprovalsUserMetrices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApprovalsUserMatrix approvalsUserMatrix)
        {
            var userid = User.GetUserId();
            approvalsUserMatrix.CreatedById = userid;
            approvalsUserMatrix.CreatedOn = DateTime.Now;

            _context.Add(approvalsUserMatrix);
            await _context.SaveChangesAsync(userid);
            TempData["SuccessMessage"] = "Approval User Matrix created successfully";
            return RedirectToAction(nameof(Index));

            ViewData["DocumentTypeId"] = new SelectList(_context.SystemCodeDetails.Include(x => x.SystemCode).Where(y => y.SystemCode.Code == "DocumentTypes"), "Id", "Description", approvalsUserMatrix.DocumentTypeId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", approvalsUserMatrix.UserId);
            ViewData["WorkFlowUserGroupId"] = new SelectList(_context.WorkFlowUserGroups, "Id", "Description", approvalsUserMatrix.WorkFlowUserGroupId);
            return View(approvalsUserMatrix);
        }

        // GET: ApprovalsUserMetrices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var approvalsUserMatrix = await _context.ApprovalsUserMatrixs.FindAsync(id);
            if (approvalsUserMatrix == null)
            {
                return NotFound();
            }
            ViewData["DocumentTypeId"] = new SelectList(_context.SystemCodeDetails.Include(x => x.SystemCode).Where(y => y.SystemCode.Code == "DocumentTypes"), "Id", "Description", approvalsUserMatrix.DocumentTypeId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", approvalsUserMatrix.UserId);
            ViewData["WorkFlowUserGroupId"] = new SelectList(_context.WorkFlowUserGroups, "Id", "Description", approvalsUserMatrix.WorkFlowUserGroupId);
            return View(approvalsUserMatrix);
        }

        // POST: ApprovalsUserMetrices/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ApprovalsUserMatrix approvalsUserMatrix)
        {
            if (id != approvalsUserMatrix.Id)
            {
                return NotFound();
            }

            var userid = User.GetUserId();
            approvalsUserMatrix.ModifiedById = userid;
            approvalsUserMatrix.ModifiedOn = DateTime.Now;

            ModelState.Remove("CreatedBy");
            ModelState.Remove("ModifiedBy");
            ModelState.Remove("DocumentType"); 
            ModelState.Remove("WorkFlowUserGroup");
            ModelState.Remove("User");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(approvalsUserMatrix);
                    await _context.SaveChangesAsync(userid);
                    TempData["Message"] = "Approval User Matrix updated successfully";

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApprovalsUserMatrixExists(approvalsUserMatrix.Id))
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
            ViewData["DocumentTypeId"] = new SelectList(_context.SystemCodeDetails.Include(x => x.SystemCode).Where(y => y.SystemCode.Code == "DocumentTypes"), "Id", "Description", approvalsUserMatrix.DocumentTypeId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", approvalsUserMatrix.UserId);
            ViewData["WorkFlowUserGroupId"] = new SelectList(_context.WorkFlowUserGroups, "Id", "Description", approvalsUserMatrix.WorkFlowUserGroupId);
            return View(approvalsUserMatrix);
        }

        // GET: ApprovalsUserMetrices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var approvalsUserMatrix = await _context.ApprovalsUserMatrixs
                .Include(a => a.DocumentType)
                .Include(a => a.User)
                .Include(a => a.WorkFlowUserGroup)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (approvalsUserMatrix == null)
            {
                return NotFound();
            }

            return View(approvalsUserMatrix);
        }

        // POST: ApprovalsUserMetrices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var approvalsUserMatrix = await _context.ApprovalsUserMatrixs.FindAsync(id);
            if (approvalsUserMatrix != null)
            {
                _context.ApprovalsUserMatrixs.Remove(approvalsUserMatrix);
            }

            await _context.SaveChangesAsync();
            TempData["DeleteMessage"] = "Approval User Matrix deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        private bool ApprovalsUserMatrixExists(int id)
        {
            return _context.ApprovalsUserMatrixs.Any(e => e.Id == id);
        }
    }
}