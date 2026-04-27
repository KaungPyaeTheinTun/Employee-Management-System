using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeesManagement.Models
{
    public class FixedAsset :UserActivity
    {
        public int Id { get; set; }
        public string AssetNo { get; set; }
        public string Description { get; set; }
        public int? CategoryId { get; set; }
        public SystemCodeDetail Category {  get; set; }
        public string SerialNo { get; set; }
        public string Model { get; set; }
        public int StatusId { get; set; }
        public SystemCodeDetail Status { get; set; }
        public int ResponsibleEmployeeId { get; set; }
        public Employee ResponsibleEmployee { get; set; }
        public int? LocationId { get; set; }
        public SystemCodeDetail Location { get; set; }
        public string? Photo { get; set; }
        public string? Notes { get; set; }
        public DateTime? PurchaseDate { get; set; }   
    }
}