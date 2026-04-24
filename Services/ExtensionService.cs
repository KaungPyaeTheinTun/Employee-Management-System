using EmployeeManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeesManagement.Services
{
    public interface IExtensionService
    {
        Task<string> GenerateEmployeeNumber();
    }

    public class ExtensionService : IExtensionService
    {

        private readonly ApplicationDbContext _context;

        public ExtensionService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<string> GenerateEmployeeNumber()
        {
            string employeeNumber;
            bool exists;
            Random _randomizer = new Random();

            do
            {
                int randomnumber = _randomizer.Next(1000, 9999);
                employeeNumber = $"EPN{randomnumber}";

                // Check if it exists
                exists = await _context.Employees.AnyAsync(e => e.EmpNo == employeeNumber);
            }
            while (exists); // keep generating until a unique number is found
            return employeeNumber;
        }

       
    }
}
