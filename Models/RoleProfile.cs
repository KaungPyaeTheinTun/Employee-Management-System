using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace EmployeesManagement.Models
{
    public class RoleProfile
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public SystemProfile Task { get; set;}
        public string RoleId { get; set;}
        public IdentityRole Role{ get; set; }
    }
}