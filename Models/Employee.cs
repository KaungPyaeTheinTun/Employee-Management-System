using System.ComponentModel;

namespace EmployeesManagement.Models
{
    public class Employee : UserActivity
    {
        public int Id { set; get; }
        public string EmpNo { set; get; }
        [DisplayName("First Name")]
        public string FirstName { set; get; }
        [DisplayName("Middle Name")]
        public string MiddleName { set; get; }
        [DisplayName("Last Name")]
        public string LastName { set; get; }
        public string FullName => $"{FirstName} {MiddleName} {LastName}";
        [DisplayName("Phone Number")]
        public string PhoneNumber { set; get; }
        [DisplayName("EmailAddress")]
        public string EmailAddress { set; get; }
        [DisplayName("Country Name")]
        public int? CountryId { set; get; }
        public Country Country { set; get; }
        public DateTime DateOfBirth { set; get; }
        public string Address { set; get; }
        [DisplayName("Department Name")]
        public int? DepartmentId { set; get; }
        public Department Department { set; get; }
        [DisplayName("Designation Name")]
        public int? DesignationId { set; get; }
        public Designation Designation { set; get; }
        [DisplayName("Gender Name")]
        public int? GenderId { set; get; }
        public SystemCodeDetail Gender { set; get; }
        [DisplayName("Employee Photo")]
        public string? Photo { set; get; }
        [DisplayName("Employment Date")]
        public DateTime? EmployeeDate { set; get; }
        public int? StatusId { set; get; }
        public SystemCodeDetail Status { set; get; }
        public DateTime? InactiveDate { set; get; }
        public int? CauseofInactivityId { set; get; }
        public SystemCodeDetail CauseofInactivity { set; get; }
        public DateTime? TerminationDate { set; get; }
        public int? ReasonforterminationId { set; get; }
        public SystemCodeDetail Reasonfortermination { set; get; }
        [DisplayName("Bank Name")]
        public int? BankId { set; get; }
        public Bank Bank { set; get; }
        [DisplayName("Bank Account No")]
        public string? BankAccountNo { set; get; }
        [DisplayName("International Bank Account Number")]
        public string? IBAN { set; get; }
        [DisplayName("Swift Code")]
        public string? SWIFTCode { set; get; }
        [DisplayName("N.S.S.F Number")]
        public string? NSSFNO { set; get; }
        [DisplayName("N.H.I.F Number")]
        public string? NHIF { set; get; }
        [DisplayName("Company Email Address")]
        public string? CompanyEmail { set; get; }
        [DisplayName("KAR PIN")]
        public string? KRAPIN { set; get; }
        [DisplayName("Passport Number")]
        public string? PassportNo { set; get; }
        [DisplayName("Employment Terms")]
        public int? EmploymentTermsId { set; get; }
        public SystemCodeDetail EmploymentTerms { set; get; }
        [DisplayName("Allocated Leave Days")]
        public Decimal? AllocatedLeaveDays { set; get; }
        [DisplayName("Leave OutStanding Balance")]
        public Decimal? LeaveOutStandingBalance { set; get; }
        public bool? PaysTax { set; get; }
        public int? DisabilityId { set; get; }
        public SystemCodeDetail Disability { set; get; }
        public string? DisabilityCertificate { set; get; }


    }
}