using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.ViewModels
{
    public class UserViewModel
    {
        public int Id { set; get; }

        [DisplayName("Email")]
        public string Email { set; get; }

        [DisplayName("FirstName")]
        public string FirstName { set; get; }

        [DisplayName("MiddleName")]
        public string MiddleName { set; get; }

        [DisplayName("LastName")]
        public string LastName { set; get; }

        [DisplayName("PhoneNumber")]
        public string PhoneNumber { set; get; }

        [DisplayName("Password")]
        public string Password { set; get; }

        [DisplayName("Address")]
        public string Address { set; get; }

        [DisplayName("UserName")]
        public string UserName{ set; get; }

        [DisplayName("NationalId")]
        public string? NationalId { get; set; }
        public string? FullName => $"{FirstName} {MiddleName} {LastName}";

        [DisplayName("RoleId")]
        public string? RoleId { get; set; }


    }
}