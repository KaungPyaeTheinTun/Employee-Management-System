using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.Models;
using EmployeeManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("/Identity/Account/Login");
        }

        var today = DateTime.Today;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        var tomorrow = today.AddDays(1);

        var totalEmployees = await _context.Employees.CountAsync();
        var activeEmployees = await _context.Employees
            .Include(e => e.Status)
            .CountAsync(e =>
                e.Status != null
                && (
                    (e.Status.Code != null && e.Status.Code.ToLower().Contains("active"))
                    || (e.Status.Description != null && e.Status.Description.ToLower().Contains("active"))
                ));

        var onLeave = await _context.LeaveApplications
            .Include(l => l.Status)
            .CountAsync(l =>
                l.StartDate <= today
                && l.EndDate >= today
                && l.Status != null
                && (
                    (l.Status.Code != null && l.Status.Code.ToLower().Contains("approval"))
                    || (l.Status.Description != null && l.Status.Description.ToLower().Contains("approval"))
                    || (l.Status.Description != null && l.Status.Description.ToLower().Contains("approved"))
                ));

        var newHires = await _context.Employees
            .CountAsync(e => e.EmployeeDate.HasValue && e.EmployeeDate.Value >= startOfMonth && e.EmployeeDate.Value < tomorrow);

        var presentToday = Math.Max(activeEmployees - onLeave, 0);

        ViewBag.TotalEmployees = totalEmployees;
        ViewBag.ActiveEmployees = presentToday;
        ViewBag.OnLeave = onLeave;
        ViewBag.NewHires = newHires;

        var departmentBreakdown = await _context.Employees
            .Include(e => e.Department)
            .Where(e => e.Department != null && !string.IsNullOrEmpty(e.Department.Name))
            .GroupBy(e => e.Department!.Name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(6)
            .ToListAsync();
        ViewBag.DepartmentBreakdown = departmentBreakdown;

        var recentEmployees = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .OrderByDescending(e => e.CreatedOn)
            .Take(5)
            .Select(e => new
            {
                e.Id,
                e.FirstName,
                e.LastName,
                Department = e.Department != null ? e.Department.Name : "-",
                Designation = e.Designation != null ? e.Designation.Name : "-",
            })
            .ToListAsync();
        ViewBag.RecentEmployees = recentEmployees;

        var leaveActivities = await _context.LeaveApplications
            .Include(l => l.Employee)
            .Include(l => l.Status)
            .OrderByDescending(l => l.CreatedOn)
            .Take(5)
            .Select(l => new
            {
                EmployeeName = l.Employee != null ? l.Employee.FirstName + " " + l.Employee.LastName : "Employee",
                Status = l.Status != null ? l.Status.Description : "Pending",
                Days = l.NoOfDays,
                l.CreatedOn
            })
            .ToListAsync();
        ViewBag.RecentLeaveActivities = leaveActivities;

        return View();
    }
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
