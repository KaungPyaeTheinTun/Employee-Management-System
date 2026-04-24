using EmployeeManagement.Data;
using EmployeeManagement.Migrations;
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
    public class LeaveApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;


        public LeaveApplicationsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var leaveapplications = await _context.LeaveApplications
                .Include(l => l.Duration)
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Include(l => l.Status)
                .ToListAsync();

            return View(leaveapplications);
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

        // GET: LeaveApplications/Create
        public IActionResult Create()
        {
            ViewData["DurationId"] = new SelectList(_context.SystemCodeDetails.Include(x => x.SystemCode).Where(y => y.SystemCode.Code == "LeaveDuration"), "Id", "Description");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName");
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name");
            return View();
        }

        [HttpGet]
        public IActionResult Approval()
        {
            var pendingApplications = _context.LeaveApplications
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Include(l => l.Status)
                .Include(l => l.Duration)
                .Where(l => l.Status.Code == "Awaiting Approval")
                .ToList();

            return View(pendingApplications);
        }
        
        [HttpPost]
        public async Task<IActionResult> ApproveLeave(LeaveApplication leave)
        {
            var approvedstatus = await _context.SystemCodeDetails
                .Include(x => x.SystemCode)
                .FirstOrDefaultAsync(y =>
                    y.SystemCode.Code == "LEAVEAPPROVALSTATUS" &&
                    y.Code == "Approval");

            var adjustmenttype = await _context.SystemCodeDetails
                .Include(x => x.SystemCode)
                .FirstOrDefaultAsync(y =>
                    y.SystemCode.Code == "LEAVEADJUSTMENT" &&
                    y.Code == "Negative");

            var leaveApplication = await _context.LeaveApplications
                .FirstOrDefaultAsync(m => m.Id == leave.Id);

            if (leaveApplication == null)
                return NotFound();

            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);

            leaveApplication.ApprovedOn = DateTime.Now;
            leaveApplication.ApprovedById = userid;
            leaveApplication.StatusId = approvedstatus.Id;

            var adjustment = new Models.LeaveAdjustmentEntry
            {
                EmployeeId = leaveApplication.EmployeeId,
                NoOfDays = leaveApplication.NoOfDays,
                LeaveStartDate = leaveApplication.StartDate,
                LeaveEndDate = leaveApplication.EndDate,
                AdjustmentDescription = "Leave Taken - Negative Adjustment",
                LeavePeriodId = 1,
                LeaveAdjustmentDate = DateTime.Now,
                AdjustmentTypeId = adjustmenttype.Id
            };

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == leaveApplication.EmployeeId);

            employee.LeaveOutStandingBalance =
                employee.AllocatedLeaveDays - leaveApplication.NoOfDays;

            _context.Update(leaveApplication);
            _context.Add(adjustment);
            _context.Update(employee);

            await _context.SaveChangesAsync(userid);

            TempData["SuccessMessage"] = "Leave approved successfully";
            return RedirectToAction("Index");   // ✅ IMPORTANT
        }

                [HttpPost]
        public async Task<IActionResult> RejectLeave(LeaveApplication leave)
        {
            var rejectedstatus = await _context.SystemCodeDetails
                .Include(x => x.SystemCode)
                .FirstOrDefaultAsync(y =>
                    y.SystemCode.Code == "LEAVEAPPROVALSTATUS" &&
                    y.Code == "Reject");

            var leaveApplication = await _context.LeaveApplications
                .FirstOrDefaultAsync(m => m.Id == leave.Id);

            if (leaveApplication == null)
                return NotFound();

            var userid = User.GetUserId();

            leaveApplication.ApprovedOn = DateTime.Now;
            leaveApplication.ApprovedById = userid;
            leaveApplication.StatusId = rejectedstatus.Id;

            _context.Update(leaveApplication);
            await _context.SaveChangesAsync(userid);

            TempData["SuccessMessage"] = "Leave rejected successfully";
            return RedirectToAction("Index");   // ✅ IMPORTANT
        }

        // POST: LeaveApplications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeaveApplication leaveApplication, IFormFile leaveattachment)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["ErrorMessage"] = string.Join(", ", errors);

                LoadDropdowns(leaveApplication);
                return View(leaveApplication);
            }
            try
    {
        // ==============================
        // ✅ GET CURRENT USER
        // ==============================
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            throw new Exception("User not found. Please login again.");

        // ==============================
        // ✅ FILE UPLOAD (OPTIONAL)
        // ==============================
        if (leaveattachment != null && leaveattachment.Length > 0)
        {
            var uploadFolder = _configuration["FileSettings:UploadFolder"];
            var path = Path.Combine(Directory.GetCurrentDirectory(), uploadFolder);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var fileName = "Leave_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + leaveattachment.FileName;
            var filePath = Path.Combine(path, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await leaveattachment.CopyToAsync(stream);
            }

            leaveApplication.Attachment = fileName;
        }

        // ==============================
        // ✅ GET STATUS (FIXED VALUE)
        // ==============================
        var pendingStatus = await _context.SystemCodeDetails
            .Include(x => x.SystemCode)
            .FirstOrDefaultAsync(y =>
                y.Code == "Awaiting Approval" &&
                y.SystemCode.Code.ToUpper() == "LEAVEAPPROVALSTATUS");

        if (pendingStatus == null)
            throw new Exception("Pending status not found in SystemCodeDetails.");

        // ==============================
        // ✅ SET SYSTEM FIELDS
        // ==============================
        leaveApplication.CreatedOn = DateTime.Now;
        leaveApplication.CreatedById = userId;

        leaveApplication.ModifiedOn = DateTime.Now;
        leaveApplication.ModifiedById = userId;

        leaveApplication.StatusId = pendingStatus.Id;

        leaveApplication.NoOfDays =
            (leaveApplication.EndDate - leaveApplication.StartDate).Days + 1;

        // ==============================
        // ✅ SAVE LEAVE APPLICATION
        // ==============================
        _context.Add(leaveApplication);
        await _context.SaveChangesAsync();

        // ==============================
        // ✅ DOCUMENT TYPE
        // ==============================
        var documentType = await _context.SystemCodeDetails
            .Include(x => x.SystemCode)
            .FirstOrDefaultAsync(x =>
                x.SystemCode.Code.ToUpper() == "DOCUMENTTYPES" &&
                x.Code == "LeaveApplication");

        if (documentType == null)
            throw new Exception("Document type not found.");

        // ==============================
        // ✅ WORKFLOW GROUP
        // ==============================
        var userGroup = await _context.ApprovalsUserMatrixs
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.DocumentTypeId == documentType.Id &&
                x.Active);

        if (userGroup == null)
            throw new Exception("Approval workflow not configured for this user.");

        // ==============================
        // ✅ APPROVERS
        // ==============================
        var approvers = await _context.WorkFlowUserGroupMembers
            .Where(x =>
                x.WorkFlowUserGroupId == userGroup.WorkFlowUserGroupId &&
                x.SenderId == userId)
            .ToListAsync();

        // ==============================
        // ✅ CREATE APPROVAL ENTRIES
        // ==============================
        foreach (var approver in approvers)
        {
            var entry = new ApprovalEntry
            {
                ApproverId = approver.ApproverId,
                DateSentForApproval = DateTime.Now,
                LastModifiedOn = DateTime.Now,
                LastModifiedById = userId,
                RecordId = leaveApplication.Id,
                ControllerName = "LeaveApplications",
                DocumentTypeId = documentType.Id,
                SequenceNo = approver.SequenceNo,
                StatusId = pendingStatus.Id,
                Comments = "Sent for Approval"
            };

            _context.Add(entry);
        }

        await _context.SaveChangesAsync();

        // ==============================
        // ✅ SUCCESS
        // ==============================
        TempData["SuccessMessage"] = "Leave Application created successfully";
        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        TempData["ErrorMessage"] = ex.Message;

        LoadDropdowns(leaveApplication);
        return View(leaveApplication);
    }
        }

        // GET: LeaveApplications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var pendingStatus = _context.SystemCodeDetails.Include(x => x.SystemCode).Where(y => y.Code == "Pending" && y.SystemCode.Code == "LeaveApprovalStatus").FirstOrDefaultAsync(); ;

            var leaveApplication = await _context.LeaveApplications.FindAsync(id);
            if (leaveApplication == null)
            {
                return NotFound();
            }
            ViewData["DurationId"] = new SelectList(_context.SystemCodeDetails.Include(x => x.SystemCode).Where(y => y.SystemCode.Code == "LeaveDuration"), "Id", "Description", leaveApplication.DurationId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", leaveApplication.EmployeeId);
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name", leaveApplication.LeaveTypeId);
            return View(leaveApplication);
        }

        // POST: LeaveApplications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LeaveApplication leaveApplication)
        {
            if (id != leaveApplication.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var pendingStatus = await _context.SystemCodeDetails
                    .Include(x => x.SystemCode)
                    .Where(y => y.Code == "Pending" && y.SystemCode.Code == "LeaveApprovalStatus")
                    .FirstOrDefaultAsync();

                try
                {
                    leaveApplication.ModifiedOn = DateTime.Now;
                    leaveApplication.ModifiedById = "Macro Code";
                    leaveApplication.StatusId = pendingStatus.Id;

                    _context.Update(leaveApplication);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeaveApplicationExists(leaveApplication.Id))
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
            ViewData["DurationId"] = new SelectList(_context.SystemCodeDetails, "Id", "Description", leaveApplication.DurationId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", leaveApplication.EmployeeId);
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name", leaveApplication.LeaveTypeId);
            return View(leaveApplication);
        }

        // GET: LeaveApplications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveApplication = await _context.LeaveApplications
                .Include(l => l.Duration)
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Include(l => l.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (leaveApplication == null)
            {
                return NotFound();
            }

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
            TempData["DeleteMessage"] = "Leave Application deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool LeaveApplicationExists(int id)
        {
            return _context.LeaveApplications.Any(e => e.Id == id);
        }

        private void LoadDropdowns(LeaveApplication leaveApplication = null)
        {
            ViewData["DurationId"] = new SelectList(_context.SystemCodeDetails.Include(x => x.SystemCode).Where(y => y.SystemCode.Code == "LeaveDuration"), "Id", "Description", leaveApplication?.DurationId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", leaveApplication?.EmployeeId);
            ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name", leaveApplication?.LeaveTypeId);
        }
    }
}
