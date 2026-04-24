using EmployeeManagement.Data;
using EmployeesManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EmployeesManagement.Services;

namespace EmployeesManagement.Controllers
{
    public class ProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
        {
            var tasks = new ProfileViewModel();
            var roles = await _context.Roles.OrderBy(x => x.Name).ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            var systemtasks = await _context.SystemProfiles
                .Include("Children.Children.Children")
                .OrderBy(x => x.Order)
                .ToListAsync();
            ViewBag.Tasks = new SelectList(systemtasks, "Id", "Name");

            return View();
        }
        public async Task<ActionResult> AssignRights(ProfileViewModel vm)
        {
            try
            {

                var userId = User.GetUserId();

                var roles = new RoleProfile
                {
                    TaskId = int.Parse(vm.TaskId),
                    RoleId = vm.RoleId,
                };
                _context.RoleProfiles.Add(roles);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Role assigned successfully";
                return RedirectToAction("Index");
            }

            catch (Exception ex)
            {
                TempData["Error"] = "Error assigning Role" + ex.Message;
                return View(vm);
            }
        }

        [HttpGet]

        public async Task<ActionResult> UserRights(string id)
        {
            var tasks = new ProfileViewModel();

            var allroles = await _context.Roles.OrderByDescending(x => x.Name).ToListAsync();
            ViewBag.RoleId = new SelectList(allroles, "Id", "Name", id);

            tasks.RoleId = id;
            tasks.Profiles = await _context.SystemProfiles
                .Include(s=>s.Profile)
                .Include("Children.Children.Children")
                .OrderBy(x => x.Order)
                .ToListAsync();

            tasks.RoleRightsIds = await _context.RoleProfiles.Where(x => x.RoleId == id).Select(r => r.TaskId).ToListAsync();

            return View(tasks);
        }

        [HttpPost]
        public async Task<ActionResult> UserGroupRights(ProfileViewModel vm)
        {
            try
            {


                var userId = User.GetUserId();


                // Remove old rights first (important)
                var existing = await _context.RoleProfiles
                    .Where(x => x.RoleId == vm.RoleId)
                    .ToListAsync();

                _context.RoleProfiles.RemoveRange(existing);

                // Add selected rights
                if (vm.Ids != null)
                {
                    foreach (var taskId in vm.Ids)
                    {
                        var role = new RoleProfile
                        {
                            TaskId = taskId,
                            RoleId = vm.RoleId,
                        };

                        _context.RoleProfiles.Add(role);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = "Rights Assigned Successfully";

                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                TempData["Error"] = "Rights could not be Assigned Successfully" + ex.Message;
                return RedirectToAction(nameof(UserRights));

            }
        }
    }
}
