using EmployeeManagement.Models;

namespace EmployeesManagement.Models
{
    public class ApprovalEntry
    {
        public int Id { get; set; }
        public int RecordId { get; set; } // 1
        public int DocumentTypeId { get; set; } // Leave Applocation
        public SystemCodeDetail DocumentType { get; set; }
        public int SequenceNo { get; set; } // 1, 2, 3 (Approvals)
        public string ApproverId { get; set; } // 1,2,3, (Approvers)
        public ApplicationUser Approver { get; set; }
        public int StatusId { get; set; } // staus of the document (Pending, Approved, Rejected)
        public SystemCodeDetail Status { get; set; }
        public DateTime DateSentForApproval { get; set; } //Date Sent For Approval
        public DateTime LastModifiedOn { get; set; } //the action of the approver (Approved, Rejected) date
        public string LastModifiedById { get; set; } //the action of the approver (Approved, Rejected) by
        public ApplicationUser LastModifiedBy { get; set; } //the action of the approver (Approved, Rejected) by
        public string Comments { get; set; } //the action of the approver (Approved, Rejected) comments
        public string ControllerName { get; set; }
    }
}