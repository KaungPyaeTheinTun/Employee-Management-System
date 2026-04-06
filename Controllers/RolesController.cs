using EmployeeManagement.Data;
using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;

        }
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles.ToListAsync();
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RoleViewModel model)
        {
            IdentityRole Role = new IdentityRole();
            Role.Name = model.RoleName;

            var result = await _roleManager.CreateAsync(Role);
            if (result.Succeeded) return RedirectToAction("Index");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string Id)
        {
            var role = new RoleViewModel();
            var result = await _roleManager.FindByIdAsync(Id);
            role.RoleName = result.Name;
            role.Id = result.Id;
            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string Id, RoleViewModel model)
        {
            var checkifexist = await _roleManager.RoleExistsAsync(model.RoleName);
            if (!checkifexist)
            {
                var result = await _roleManager.FindByIdAsync(Id);
                result.Name = model.RoleName;

                var finalResult = await _roleManager.UpdateAsync(result);
                if (finalResult.Succeeded) return RedirectToAction("Index");
                return View(model);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
                return NotFound();

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
                return RedirectToAction("Index");

            return View("Index");
        }
    }
}