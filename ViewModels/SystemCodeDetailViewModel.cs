using EmployeesManagement.Models;

namespace EmployeeManagement.ViewModels
{
    public class SystemCodeDetailViewModel
    {
        public int Id { get; set; }
        public int SystemCodeId { get; set; }
        public SystemCode SystemCode { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int? OrderNo { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<SystemCodeDetail> SystemCodeDetails { get; set; }
    }
}