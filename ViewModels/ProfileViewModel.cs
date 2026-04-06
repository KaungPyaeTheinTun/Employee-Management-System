using System.ComponentModel;
using EmployeesManagement.Models;

namespace EmployeeManagement.ViewModels
{
    public class ProfileViewModel
    {
        public ICollection<SystemProfile> Profiles { get; set; }
        public ICollection<int> RoleRightsIds { get; set; }
        public int[] Ids { get; set; }
        [DisplayName("Role")]
        public string RoleId { get; set; }
        [DisplayName("System Task")]
        public string TaskId { get; set; }
    }
}