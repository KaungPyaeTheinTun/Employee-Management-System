using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EmployeeManagement.Data;
using EmployeeManagement.ViewModels;
using EmployeesManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Controllers
{
    public class ProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var tasks = new ProfileViewModel();
            var roles = await _context.Roles.OrderBy(x => x.Name).ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            var systemtasks = await _context.SystemProfiles
                            .Include("Children.Children.Children")
                            .OrderBy(x => x.Name)
                            .ToListAsync();

            ViewBag.Tasks = new SelectList(systemtasks, "Id", "Name");

            return View(tasks);
        }

        public async Task<IActionResult> AssignRights(ProfileViewModel vm)
        {
            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = new RoleProfile
            {
                TaskId = int.Parse(vm.TaskId),
                RoleId = vm.RoleId
            };

            _context.RoleProfiles.Add(role);
            await _context.SaveChangesAsync(UserId);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> UserRights(string id)
        {
            var tasks = new ProfileViewModel();
            tasks.RoleId = id;

            var Profiles = await _context.SystemProfiles
                .Include(s => s.Profile)
                .Include("Children.Children.Children")
                .OrderBy(x => x.Order)
                .ToListAsync();

            tasks.Profiles = Profiles;

            tasks.RoleRightsIds = await _context.RoleProfiles
                .Where(x => x.RoleId == id)
                .Select(r => r.TaskId)
                .ToListAsync();

            return View(tasks);
        }

        [HttpPost]
        [HttpPost]
        public async Task<ActionResult> UserGroupRights(string id, ProfileViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var allrights = await _context.RoleProfiles
                .Where(x => x.RoleId == vm.RoleId)
                .ToListAsync();

            _context.RoleProfiles.RemoveRange(allrights);

            if (vm.Ids != null && vm.Ids.Any())
            {
                foreach (var taskId in vm.Ids)
                {
                    _context.RoleProfiles.Add(new RoleProfile
                    {
                        TaskId = taskId,
                        RoleId = vm.RoleId
                    });
                }
            }

            await _context.SaveChangesAsync(userId);

            return RedirectToAction("UserRights", new { id = vm.RoleId });
        }
    }
}