using AutoMapper;
using LinkDev.Talabat.Core.Application;
using LinkDev.Talabat.Core.Application.Services.Employees;
using LinkDev.Talabat.Core.Domain.Contracts;
using LinkDev.Talabat.Core.Domain.Contracts.Persistence;
using LinkDev.Talabat.Core.Domain.Entities.Employees;
using Moq;

namespace LinkDev.Talabat.Tests
{
    public class EmployeeServiceTests
    {
        [Fact]
        public async Task GetEmployeeAsync_WhenMissing_ThrowsNotFound()
        {
            var employees = new Mock<IGenericRepository<Employee, int>>();
            employees.Setup(r => r.GetWithSpecAsync(It.IsAny<ISpecfifcations<Employee, int>>()))
                     .ReturnsAsync((Employee?)null);
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(u => u.GetRepository<Employee, int>()).Returns(employees.Object);

            var sut = new EmployeeService(unitOfWork.Object, new Mock<IMapper>().Object);

            await Assert.ThrowsAsync<NotFoundException>(() => sut.GetEmployeeAsync(1));
        }
    }
}
