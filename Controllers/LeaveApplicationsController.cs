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
    public class LeaveApplicationsController : Controller 
    { 
        private readonly ApplicationDbContext _context; 
        private readonly IConfiguration _configuration;
 
        public LeaveApplicationsController(ApplicationDbContext context, IConfiguration configuration) 
        { 
            _context = context; 
            _configuration = configuration;
        } 
 
        // GET: LeaveApplications 
        public async Task<IActionResult> Index() 
        { 
            var applicationDbContext = _context.LeaveApplications 
                .Include(l => l.Duration) 
                .Include(l => l.Employee) 
                .Include(l => l.LeaveType) 
                .Include(l => l.Status); 
            return View(await applicationDbContext.ToListAsync()); 
        } 
 
        // GET: LeaveApplications/Details/5 
        public async Task<IActionResult> Details(int? id) 
        { 
            if (id == null) return NotFound(); 
 
            var leaveApplication = await _context.LeaveApplications 
                .Include(l => l.Duration) 
                .Include(l => l.Employee) 
                .Include(l => l.LeaveType) 
                .Include(l => l.Status) 
                .FirstOrDefaultAsync(m => m.Id == id); 
 
            if (leaveApplication == null) return NotFound(); 
 
            return View(leaveApplication); 
        } 

        //GET Approval View
        [HttpGet]
        public IActionResult Approval()
        {
            var pendingApplications = _context.LeaveApplications
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Include(l => l.Status)
                .Include(l => l.Duration)
                .Where(l => l.Status.Code == "Pending")
                .ToList();

            return View(pendingApplications);
        }

        //POST Approval View
        [HttpPost]
        public async Task<IActionResult> ApproveLeave(int id)
        {
            var ApproveStatus = await _context.SystemCodeDetails
                .Include(x => x.SystemCode)
                .Where(y => y.SystemCode.Code == "LEAVEAPPROVALSTATUS" && y.Code == "Approval")
                .FirstOrDefaultAsync();
            
            var adjustmentType = await _context.SystemCodeDetails
                .Include(x => x.SystemCode)
                .Where(y => y.SystemCode.Code == "LEAVEADJUSTMENT" && y.Code == "Negative")
                .FirstOrDefaultAsync();

            var leaveApplication = await _context.LeaveApplications
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Include(l => l.Status)
                .Include(l => l.Duration)
                .FirstOrDefaultAsync(m => m.Id == id);; 

            if(leaveApplication == null) return NotFound();

            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            leaveApplication.ApprovedOn = DateTime.Now;
            leaveApplication.ApprovedById = UserId;
            leaveApplication.StatusId = ApproveStatus.Id;

            _context.Update(leaveApplication);
            await _context.SaveChangesAsync(UserId);

            var adjustment = new LeaveAdjustmentEntry
            {
                EmployeeId = leaveApplication.EmployeeId,
                NoOfDays = leaveApplication.NoOfDays,
                LeaveStartDate = leaveApplication.StartDate,
                LeaveEndDate = leaveApplication.EndDate,
                AdjustmentDescription = "Leave Taken - Negative Adjustment.",
                LeavePeriodId = 1,
                LeaveAdjustmentDate = DateTime.Now,
                AdjustmentTypeId = adjustmentType.Id
            };

            _context.Add(adjustment);
            await _context.SaveChangesAsync(UserId);

            var employee = await _context.Employees.FindAsync(leaveApplication.EmployeeId);
            employee.LeaveOutStandingBalance = employee.AllocatedLeaveDays - leaveApplication.NoOfDays;
            _context.Update(employee);
            await _context.SaveChangesAsync(UserId);

            ViewData["DurationId"] = new SelectList(_context.SystemCodeDetails.Where(y => y.SystemCode.Code == "LeaveDuration"), "Id", "Description", leaveApplication.DurationId); 
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName"); 
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RejectLeave(int id)
        {
            var ApproveStatus = await _context.SystemCodeDetails
                .Include(x => x.SystemCode)
                .Where(y => y.SystemCode.Code == "LEAVEAPPROVALSTATUS" && y.Code == "Reject")
                .FirstOrDefaultAsync();

            var leaveApplication = await _context.LeaveApplications
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Include(l => l.Status)
                .Include(l => l.Duration)
                .FirstOrDefaultAsync(m => m.Id == id);; 

            if(leaveApplication == null) return NotFound();

            leaveApplication.ApprovedOn = DateTime.Now;
            leaveApplication.ApprovedById = "Kptt";
            leaveApplication.StatusId = ApproveStatus.Id;

            _context.Update(leaveApplication);
            await _context.SaveChangesAsync();

            ViewData["DurationId"] = new SelectList(_context.SystemCodeDetails.Where(y => y.SystemCode.Code == "LeaveDuration"), "Id", "Description", leaveApplication.DurationId); 
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName"); 
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name");
            return RedirectToAction(nameof(Index));
        }

        // GET: LeaveApplications/Create 
        public async Task<IActionResult> Create() 
        {
            ViewData["DurationId"] = new SelectList(_context.SystemCodeDetails
                                    .Include(x => x.SystemCode)
                                    .Where(y => y.SystemCode.Code == "LeaveDuration"),
                                     "Id", "Description"); 
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName"); 
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name"); 
 
            return View(); 
        } 
 
        // POST: LeaveApplications/Create 
        [HttpPost] 
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Create(LeaveApplication leaveApplication, IFormFile leaveattachment) 
        { 
            if (leaveattachment != null && leaveattachment.Length > 0)
            {
                var uploadFolder = _configuration["FileSettings:UploadFolder"];

                var path = Path.Combine(Directory.GetCurrentDirectory(), uploadFolder);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var fileName = "EmployeePhoto_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + leaveattachment.FileName;

                var filePath = Path.Combine(path, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await leaveattachment.CopyToAsync(stream);
                }

                leaveApplication.Attachment = fileName; 
            }
                var pendingStatus = await _context.SystemCodeDetails 
                    .Include(x => x.SystemCode) 
                    .Where(y => y.Code == "Pending" && y.SystemCode.Code == "LeaveApprovalStatus") 
                    .FirstOrDefaultAsync(); 

                var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
 
                leaveApplication.CreatedOn = DateTime.Now; 
                leaveApplication.CreatedById = "Marco Code"; 
                leaveApplication.StatusId = pendingStatus?.Id ?? 0; 
 
                _context.Add(leaveApplication); 
                await _context.SaveChangesAsync(UserId); 
                return RedirectToAction(nameof(Index)); 

 
            // Re-populate dropdowns if ModelState invalid 
            var durations = await _context.SystemCodeDetails 
                .Include(x => x.SystemCode) 
                .Where(y => y.SystemCode.Code == "LeaveDuration") 
                .ToListAsync(); 
            var employees = await _context.Employees.ToListAsync(); 
            var leaveTypes = await _context.LeaveTypes.ToListAsync(); 
            var statuses = await _context.SystemCodeDetails 
                .Include(x => x.SystemCode) 
                .Where(y => y.SystemCode.Code == "LeaveApprovalStatus")
                .ToListAsync(); 
 
            ViewData["DurationId"] = new SelectList(durations, "Id", "Description", leaveApplication.DurationId); 
            ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", leaveApplication.EmployeeId); 
            ViewData["LeaveTypeId"] = new SelectList(leaveTypes, "Id", "Name", leaveApplication.LeaveTypeId); 
 
            return View(leaveApplication); 
        } 
 
        // GET: LeaveApplications/Edit/5 
        public async Task<IActionResult> Edit(int? id) 
        { 
            if (id == null) return NotFound(); 
 
            var leaveApplication = await _context.LeaveApplications.FindAsync(id); 
            if (leaveApplication == null) return NotFound(); 
 
            var durations = await _context.SystemCodeDetails 
                .Include(x => x.SystemCode) 
                .Where(y => y.SystemCode.Code == "LeaveDuration") 
                .ToListAsync(); 
            var employees = await _context.Employees.ToListAsync(); 
            var leaveTypes = await _context.LeaveTypes.ToListAsync(); 
 
            ViewData["DurationId"] = new SelectList(durations, "Id", "Description", leaveApplication.DurationId); 
            ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", leaveApplication.EmployeeId); 
            ViewData["LeaveTypeId"] = new SelectList(leaveTypes, "Id", "Name", leaveApplication.LeaveTypeId); 
 
            return View(leaveApplication); 
        } 
 
        // POST: LeaveApplications/Edit/5 
        [HttpPost] 
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Edit(int id, LeaveApplication leaveApplication) 
        { 
            if (id != leaveApplication.Id) return NotFound(); 
 
            if (ModelState.IsValid) 
            { 
                var pendingStatus = await _context.SystemCodeDetails 
                    .Include(x => x.SystemCode) 
                    .Where(y => y.Code == "Pending" && y.SystemCode.Code == "LeaveApprovalStatus") 
                    .FirstOrDefaultAsync(); 
 
                try 
                { 
                    leaveApplication.ModifiedOn = DateTime.Now; 
                    leaveApplication.ModifiedById = "Marco Code"; 
                    leaveApplication.StatusId = pendingStatus.Id; 
 
                    _context.Update(leaveApplication); 
                    await _context.SaveChangesAsync(); 
                } 
                catch (DbUpdateConcurrencyException) 
                { 
                    if (!LeaveApplicationExists(leaveApplication.Id)) return NotFound(); 
                    else throw; 
                } 
                return RedirectToAction(nameof(Index)); 
            } 
 
            // Re-populate dropdowns if ModelState invalid 
            var durations = await _context.SystemCodeDetails 
                .Include(x => x.SystemCode) 
                .Where(y => y.SystemCode.Code == "LeaveDuration") 
                .ToListAsync(); 
            var employees = await _context.Employees.ToListAsync(); 
            var leaveTypes = await _context.LeaveTypes.ToListAsync(); 
 
            ViewData["DurationId"] = new SelectList(durations, "Id", "Description", leaveApplication.DurationId); 
            ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", leaveApplication.EmployeeId); 
            ViewData["LeaveTypeId"] = new SelectList(leaveTypes, "Id", "Name", leaveApplication.LeaveTypeId); 
 
            return View(leaveApplication); 
        } 
 
        // GET: LeaveApplications/Delete/5 
        public async Task<IActionResult> Delete(int? id) 
        { 
            if (id == null) return NotFound(); 
 
            var leaveApplication = await _context.LeaveApplications 
                .Include(l => l.Duration) 
                .Include(l => l.Employee) 
                .Include(l => l.LeaveType) 
                .Include(l => l.Status)
                .FirstOrDefaultAsync(m => m.Id == id); 
 
            if (leaveApplication == null) return NotFound(); 
 
            return View(leaveApplication); 
        } 
 
        // POST: LeaveApplications/Delete/5 
        [HttpPost, ActionName("Delete")] 
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> DeleteConfirmed(int id) 
        { 
            var leaveApplication = await _context.LeaveApplications.FindAsync(id); 
            if (leaveApplication != null) 
            { 
                _context.LeaveApplications.Remove(leaveApplication); 
            } 
 
            await _context.SaveChangesAsync(); 
            return RedirectToAction(nameof(Index)); 
        } 
 
        private bool LeaveApplicationExists(int id) 
        { 
            return _context.LeaveApplications.Any(e => e.Id == id); 
        } 
    } 
}