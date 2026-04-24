using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeesManagement.Models
{
    public class LeaveType : UserActivity
    {
        public int Id { get; set; }
        public string Name { get; set; }    
        public string Code { get; set; }
        public decimal Days { get; set;}
    }
}