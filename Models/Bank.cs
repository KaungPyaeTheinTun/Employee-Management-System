using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeesManagement.Models
{
    public class Bank : UserActivity
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }    
        public string AccoutNo { get; set; }
    }
}