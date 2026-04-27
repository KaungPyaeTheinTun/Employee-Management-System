using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Data;
using EmployeesManagement.Models;
using EmployeesManagement.Services;
using System.Security.Claims;

namespace EmployeesManagement.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Clients
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Clients.Include(c => c.CreatedBy).Include(c => c.ModifiedBy).Include(c => c.Status);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.CreatedBy)
                .Include(c => c.ModifiedBy)
                .Include(c => c.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            try
            {
                var statusId = await _context.SystemCodeDetails
                        .Include(x => x.SystemCode)
                        .Where(x => x.SystemCode.Code == "CLIENTSTATUS"
                        && x.Code == "Active").FirstOrDefaultAsync();
                if (statusId == null)
                    TempData["ErrorMessage"] = "Client Status 'Active' not found.";
                var Userid = User.GetUserId();
                client.CreatedOn = DateTime.Now;
                client.CreatedById = Userid;
                client.StatusId = statusId.Id;
                _context.Add(client);
                await _context.SaveChangesAsync(Userid);
                TempData["SuccessMessage"] = "Client Details created successfully";
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Client Details could not be created successfully" + ex.Message;
                return View(client);
            }   
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                        .Include(c => c.CreatedBy)
                        .Include(c => c.ModifiedBy)
                        .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }
           
            return View(client);
        }

        // POST: Clients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.Id)
            {
                return NotFound();
            }

          
                try
                {
                var Userid = User.GetUserId();
                client.ModifiedOn = DateTime.Now;
                client.ModifiedById = Userid;
                _context.Update(client);
                    await _context.SaveChangesAsync(Userid);
                TempData["SuccessMessage"] = "Client Details updated successfully";

                return RedirectToAction(nameof(Index));

            }
            catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            
           
            return View(client);
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.CreatedBy)
                .Include(c => c.ModifiedBy)
                .Include(c => c.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Client Details deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}
