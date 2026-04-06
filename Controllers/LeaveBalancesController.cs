using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EmployeeManagement.Data;
using EmployeesManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Controllers
{
    public class LeaveBalancesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public LeaveBalancesController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var result = await _context.Employees
                        .Include(x => x.Status)
                        .ToListAsync();

            return View(result);
        }
        [HttpGet]
        public IActionResult adjustleavebalance(int id)
        {
            LeaveAdjustmentEntry leaveAdjustment = new();
            leaveAdjustment.EmployeeId = id;
            ViewData["AdjustmentTypeId"] = new SelectList(_context.SystemCodeDetails
                                            .Include(y => y.SystemCode)
                                            .Where(x => x.SystemCode.Code == "LEAVEADJUSTMENT"), "Id", "Description");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", id);
            ViewData["LeavePeriodId"] = new SelectList(_context.LeavePeriods
                                        .Where(x => x.Closed == false), "Id", "Name");
            return View(leaveAdjustment);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> adjustleavebalance(LeaveAdjustmentEntry leaveAdjustmentEntry)
        {
            var adjustmentType = await _context.SystemCodeDetails
                                .Include(x => x.SystemCode)
                                .Where(y => y.SystemCode.Code == "LEAVEADJUSTMENT" && y.Id == leaveAdjustmentEntry.AdjustmentTypeId)
                                .FirstOrDefaultAsync();

            leaveAdjustmentEntry.AdjustmentDescription = leaveAdjustmentEntry.AdjustmentDescription + "-" + adjustmentType.Description;
            leaveAdjustmentEntry.Id = 0;
            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.Add(leaveAdjustmentEntry);
            await _context.SaveChangesAsync(UserId);

            var employee = await _context.Employees.FindAsync(leaveAdjustmentEntry.EmployeeId);
            if (adjustmentType.Description == "Positive")
            {
                employee.LeaveOutStandingBalance = employee.AllocatedLeaveDays + leaveAdjustmentEntry.NoOfDays;
            }
            else
            {
                employee.LeaveOutStandingBalance = employee.AllocatedLeaveDays - leaveAdjustmentEntry.NoOfDays;
            }
            _context.Update(employee);
            await _context.SaveChangesAsync(UserId);

            return RedirectToAction(nameof(Index));
            
            ViewData["LeavePeriodId"] = new SelectList(_context.LeavePeriods
                                        .Where(x => x.Closed == false)
                                        , "Id", "Name",leaveAdjustmentEntry.LeavePeriodId);
            ViewData["AdjustmentTypeId"] = new SelectList(_context.SystemCodeDetails, "Id", "Description", leaveAdjustmentEntry.AdjustmentTypeId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", leaveAdjustmentEntry.EmployeeId);
            return View(leaveAdjustmentEntry);
        }
    }
}