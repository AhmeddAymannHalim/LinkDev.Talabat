using AutoMapper;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Employees;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Employees;
using LinkDev.Talabat.Core.Domain.Contracts.Persistence;
using LinkDev.Talabat.Core.Domain.Entities.Employees;
using LinkDev.Talabat.Core.Domain.Specifications.Employee_Specs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Application.Services.Employees
{
    public class EmployeeService(IUnitOfWork unitOfWork, IMapper mapper) : IEmployeeService
    {
        public async Task<EmployeeToReturnDto> GetEmployeeAsync(int id)
        {
            var spec = new EmployeeWithDepartmentSpecifications(id);

            var employee  = await unitOfWork.GetRepository<Employee,int>().GetWithSpecAsync(spec);

            return mapper.Map<EmployeeToReturnDto>(employee);

        }

        public async Task<IEnumerable<EmployeeToReturnDto>> GetEmployeesAsync()
        {
            var spec = new EmployeeWithDepartmentSpecifications();

            var employees = await unitOfWork.GetRepository<Employee, int>().GetAllWithSpecAsync(spec);

            return mapper.Map<IEnumerable<EmployeeToReturnDto>>(employees);
        }
    }
}
