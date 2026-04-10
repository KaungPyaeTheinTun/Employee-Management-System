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
         // POST: LeaveApplications/Create 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeaveApplication leaveApplication, IFormFile leaveattachment)
        {
            try
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

                // 2. GET LOGIN USER
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User not logged in");

                // 3. GET STATUS (Awaiting Approval)
                var pendingStatus = await _context.SystemCodeDetails
                    .Include(x => x.SystemCode)
                    .FirstOrDefaultAsync(y =>
                        y.Code == "Awaiting Approval" &&
                        y.SystemCode.Code == "LEAVEAPPROVALSTATUS");

                if (pendingStatus == null)
                    throw new Exception("Status 'Awaiting Approval' not found");

                // 4. SAVE LEAVE APPLICATION
                leaveApplication.CreatedOn = DateTime.Now;
                leaveApplication.CreatedById = userId;
                leaveApplication.StatusId = pendingStatus.Id;

                _context.Add(leaveApplication);
                await _context.SaveChangesAsync(userId);

                // 5. GET DOCUMENT TYPE
                var documenttype = await _context.SystemCodeDetails
                    .Include(x => x.SystemCode)
                    .FirstOrDefaultAsync(x =>
                        x.SystemCode.Code == "DOCUMENTTYPES" &&
                        x.Code == "LeaveApplication"); // MUST match DB

                if (documenttype == null)
                    throw new Exception("DocumentType 'LEAVE_APPLICATION' not found");

                // 6. GET USER WORKFLOW GROUP
                var usergroup = await _context.ApprovalsUserMatrixs
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId &&
                        x.DocumentTypeId == documenttype.Id &&
                        x.Active == true);

                if (usergroup == null)
                    throw new Exception("User is not assigned to any workflow group");

                // 7. GET APPROVERS
                var approvers = await _context.WorkFlowUserGroupMembers
                    .Where(x =>
                        x.WorkFlowUserGroupId == usergroup.WorkFlowUserGroupId &&
                        x.SenderId == userId)
                    .ToListAsync();

                if (approvers == null || !approvers.Any())
                    throw new Exception("No approvers found for this workflow group");

                // 8. INSERT APPROVAL ENTRIES
                foreach (var approver in approvers)
                {
                    var approvalEntry = new ApprovalEntry
                    {
                        ApproverId = approver.ApproverId,
                        RecordId = leaveApplication.Id,
                        DocumentTypeId = documenttype.Id,
                        SequenceNo = approver.SequenceNo,
                        StatusId = pendingStatus.Id,
                        Comments = "Sent for Approval",
                        ControllerName = "LeaveApplications",
                        DateSentForApproval = DateTime.Now,
                        LastModifiedOn = DateTime.Now,
                        LastModifiedById = userId
                    };

                    _context.Add(approvalEntry);
                }

                // ✅ Save ONCE (important)
                await _context.SaveChangesAsync(userId);

                // 9. REDIRECT
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // ERROR HANDLING
                ModelState.AddModelError("", ex.Message);

                // Reload dropdowns
                ViewData["DurationId"] = new SelectList(
                    _context.SystemCodeDetails.Include(x => x.SystemCode)
                        .Where(y => y.SystemCode.Code == "LEAVEDURATION"),
                    "Id", "Description", leaveApplication.DurationId);

                ViewData["EmployeeId"] = new SelectList(
                    _context.Employees, "Id", "FullName", leaveApplication.EmployeeId);

                ViewData["LeaveTypeId"] = new SelectList(
                    _context.LeaveTypes, "Id", "Name", leaveApplication.LeaveTypeId);

                return View(leaveApplication);
            }
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