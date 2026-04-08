namespace EmployeesManagement.Models
{
    public class CompanyInformation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNo { get; set; }
        public string NSSFNO { get; set; }
        public string NHIFNO { get; set; }
        public string KRAPIN { get; set; }
        public string ContactPerson { get; set; }
        public string Logo { get; set; }
        public string PostalCode { get; set; }
        public int CityId { get; set; }
        public City City { get; set; }
        public int CountryId { get; set; }
        public Country Country { get; set; }    
    }
}