namespace EmployeesManagement.Models
{
    public class LeaveApplication : ApprovalActivity
    {
        public int Id { set; get; }
        public int EmployeeId { set; get; }
        public Employee? Employee { set; get; }
        public int NoOfDays { set; get; }
        public DateTime StartDate { set; get; }
        public DateTime EndDate { set; get; }
        public int DurationId { set; get; }
        public SystemCodeDetail? Duration { get; set; }
        public int LeaveTypeId { set; get; }
        public LeaveType? LeaveType { get; set; }
        public string? Attachment { get; set; }
        public string Description { get; set; }
        public int StatusId { get; set; }
        public SystemCodeDetail? Status { get; set; }

    }
}