using AutoMapper;
using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using EmployeesManagement.Models;

namespace EmployeeManagement.Profiles
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Employee, EmployeeViewModel>().ReverseMap();
        }
    }
}