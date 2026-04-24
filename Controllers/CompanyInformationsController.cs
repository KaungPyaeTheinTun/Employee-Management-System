using EmployeeManagement.Data;
using EmployeesManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CompanyInformation = EmployeesManagement.Models.CompanyInformation;

namespace EmployeesManagement.Controllers
{
    public class CompanyInformationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public CompanyInformationsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: CompanyInformations
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.CompanyInformations.Include(c => c.City).Include(c => c.Country);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: CompanyInformations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyInformation = await _context.CompanyInformations
                .Include(c => c.City)
                .Include(c => c.Country)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (companyInformation == null)
            {
                return NotFound();
            }

            return View(companyInformation);
        }

        // GET: CompanyInformations/Create
        public IActionResult Create()
        {
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name");
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Name");
            return View();
        }

        // POST: CompanyInformations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanyInformation companyInformation, IFormFile logo)
        {
            var userid = User.GetUserId();

            if (logo != null && logo.Length > 0)
            {
                var uploadFolder = _configuration["FileSettings:UploadFolder"];

                var path = Path.Combine(Directory.GetCurrentDirectory(), uploadFolder);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var fileName = "logo_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + logo.FileName;

                var filePath = Path.Combine(path, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await logo.CopyToAsync(stream);
                }

                companyInformation.Logo = fileName;
            }
            _context.Add(companyInformation);
                await _context.SaveChangesAsync(userid);
            TempData["SuccessMessage"] = "Company information created successfully.";

                return RedirectToAction(nameof(Index));

            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", companyInformation.CityId);
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Name", companyInformation.CountryId);
            return View(companyInformation);
        }

        // GET: CompanyInformations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyInformation = await _context.CompanyInformations.FindAsync(id);
            if (companyInformation == null)
            {
                return NotFound();
            }
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", companyInformation.CityId);
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Name", companyInformation.CountryId);
            return View(companyInformation);
        }
        // POST: CompanyInformations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,PhoneNo,NSSFNO,NHIFNO,KRAPIN,ContactPerson,Logo,PostalCode,CityId,CountryId")] CompanyInformation companyInformation)
        {
            if (id != companyInformation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(companyInformation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyInformationExists(companyInformation.Id))
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
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", companyInformation.CityId);
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Name", companyInformation.CountryId);
            return View(companyInformation);
        }

        // GET: CompanyInformations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companyInformation = await _context.CompanyInformations
                .Include(c => c.City)
                .Include(c => c.Country)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (companyInformation == null)
            {
                return NotFound();
            }

            return View(companyInformation);
        }

        // POST: CompanyInformations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var companyInformation = await _context.CompanyInformations.FindAsync(id);
            if (companyInformation != null)
            {
                _context.CompanyInformations.Remove(companyInformation);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Company information deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private bool CompanyInformationExists(int id)
        {
            return _context.CompanyInformations.Any(e => e.Id == id);
        }
    }
}