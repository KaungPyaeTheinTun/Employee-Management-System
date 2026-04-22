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
using EmployeeManagement.ViewModels;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using AutoMapper;

namespace EmployeeManagement.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public EmployeesController(IMapper mapper, ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _mapper = mapper;
        }

        // GET: Employees
        public async Task<IActionResult> Index(EmployeeViewModel employees)
        {
            var rawdata = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Include(e => e.Gender)
                .Include(x => x.Status)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(employees.SearchTerm))
            {
                var searchTerm = employees.SearchTerm.Trim();

                rawdata = rawdata.Where(x =>
                    (x.EmpNo != null && x.EmpNo.Contains(searchTerm)) ||

                    (x.FirstName != null && x.FirstName.Contains(searchTerm)) ||
                    (x.MiddleName != null && x.MiddleName.Contains(searchTerm)) ||
                    (x.LastName != null && x.LastName.Contains(searchTerm)) ||

                    (x.PhoneNumber != null && x.PhoneNumber.Contains(searchTerm)) ||
                    (x.EmailAddress != null && x.EmailAddress.Contains(searchTerm))
                );
            }

            employees.Employees = await rawdata.ToListAsync();
            return View(employees);
        }
        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(x => x.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            ViewData["BankId"] = new SelectList(_context.Banks, "Id", "Name");
            ViewData["EmploymentTermsId"] = new SelectList(_context.SystemCodeDetails
                                    .Include(x => x.SystemCode)
                                    .Where(x => x.SystemCode.Code == "EmploymentTerms"), "Id", "Description");
            ViewData["DisabilityId"] = new SelectList(_context.SystemCodeDetails
                                    .Include(x => x.SystemCode)
                                    .Where(x => x.SystemCode.Code == "DisabilityTypes"), "Id", "Description");
            ViewData["GenderId"] = new SelectList(_context.SystemCodeDetails
                                    .Include(x => x.SystemCode)
                                    .Where(x => x.SystemCode.Code == "Gender"), "Id", "Description");
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Name");
            ViewData["DesignationId"] = new SelectList(_context.Designations, "Id", "Name");
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel newemployee, IFormFile employeephoto)
        {
            var employee = new Employee();
            _mapper.Map(newemployee, employee);

            if (employeephoto != null && employeephoto.Length > 0)
            {
                var uploadFolder = _configuration["FileSettings:UploadFolder"];

                var path = Path.Combine(Directory.GetCurrentDirectory(), uploadFolder);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var fileName = "EmployeePhoto_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + employeephoto.FileName;

                var filePath = Path.Combine(path, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await employeephoto.CopyToAsync(stream);
                }

                employee.Photo = fileName;
            }

            var statusId = await _context.SystemCodeDetails
                                    .Include(x => x.SystemCode)
                                    .Where(x => x.SystemCode.Code == "EmployeeStatus" && x.Code == "Active").FirstOrDefaultAsync();

            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            employee.CreatedOn = DateTime.Now;
            employee.CreatedById = UserId;
            employee.StatusId = statusId.Id;

            _context.Add(employee);
            await _context.SaveChangesAsync(UserId);
            return RedirectToAction(nameof(Index));

            ViewData["BankId"] = new SelectList(_context.Banks, "Id", "Name", employee.BankId);
            ViewData["EmploymentTermsId"] = new SelectList(_context.SystemCodeDetails
                                    .Include(x => x.SystemCode)
                                    .Where(x => x.SystemCode.Code == "EmploymentTerms"), "Id", "Description", employee.EmploymentTermsId);
            ViewData["DisabilityId"] = new SelectList(_context.SystemCodeDetails
                                    .Include(x => x.SystemCode)
                                    .Where(x => x.SystemCode.Code == "DisabilityTypes"), "Id", "Description", employee.DisabilityId);
            ViewData["GenderId"] = new SelectList(_context.SystemCodeDetails
                                    .Include(x => x.SystemCode)
                                    .Where(x => x.SystemCode.Code == "Gender"), "Id", "Description", employee.GenderId);
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Name", employee.CountryId);
            ViewData["DesignationId"] = new SelectList(_context.Designations, "Id", "Name", employee.DesignationId);
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            ViewData["GenderId"] = new SelectList(_context.SystemCodeDetails
                                    .Include(x => x.SystemCode)
                                    .Where(x => x.SystemCode.Code == "Gender"), "Id", "Description");
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Name");
            ViewData["DesignationId"] = new SelectList(_context.Designations, "Id", "Name");
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            ViewData["BankId"] = new SelectList(_context.Banks, "Id", "Name");
            ViewData["EmploymentTermsId"] = new SelectList(_context.SystemCodeDetails
                                    .Include(x => x.SystemCode)
                                    .Where(x => x.SystemCode.Code == "EmploymentTerms"), "Id", "Description");
            ViewData["DisabilityId"] = new SelectList(_context.SystemCodeDetails
                                    .Include(x => x.SystemCode)
                                    .Where(x => x.SystemCode.Code == "DisabilityTypes"), "Id", "Description");
            

            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Bank");
            ModelState.Remove("Country");
            ModelState.Remove("Department");
            ModelState.Remove("Designation");
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("Status");
            ModelState.Remove("Gender");
            ModelState.Remove("EmploymentTerms");
            ModelState.Remove("Disability");
            ModelState.Remove("CauseofInactivity");
            ModelState.Remove("Reasonfortermination");
            ModelState.Remove("CreatedBy");
            ModelState.Remove("ModifiedBy");
            ModelState.Remove("Employee");


            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                employee.ModifiedOn = DateTime.Now;
                employee.ModifiedById = userId;
                _context.Update(employee);
                await _context.SaveChangesAsync(userId);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(employee.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));

            ViewData["GenderId"] = new SelectList(_context.SystemCodeDetails
                                    .Include(x => x.SystemCode)
                                    .Where(x => x.SystemCode.Code == "Gender"), "Id", "Description", employee.GenderId);
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Name", employee.CountryId);
            ViewData["DesignationId"] = new SelectList(_context.Designations, "Id", "Name", employee.DesignationId);
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
            ViewData["BankId"] = new SelectList(_context.Banks, "Id", "Name", employee.BankId);
            ViewData["EmploymentTermsId"] = new SelectList(_context.SystemCodeDetails
                                    .Include(x => x.SystemCode)
                                    .Where(x => x.SystemCode.Code == "EmploymentTerms"), "Id", "Description", employee.EmploymentTermsId);
            ViewData["DisabilityId"] = new SelectList(_context.SystemCodeDetails
                                    .Include(x => x.SystemCode)
                                    .Where(x => x.SystemCode.Code == "DisabilityTypes"), "Id", "Description", employee.DisabilityId);

            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
