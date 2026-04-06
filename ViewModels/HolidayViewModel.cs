using System.ComponentModel;
using EmployeesManagement.Models;

namespace EmployeeManagement.ViewModels
{
    public class HolidayViewModel : UserActivity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public Holiday Holiday{ get; set; }
        public List<Holiday> Holidays { get; set; }
        public string? SearchTerm { get; set; }
        
    }
}