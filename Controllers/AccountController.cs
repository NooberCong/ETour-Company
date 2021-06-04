using Company.Models;
using Core.Interfaces;
using Infrastructure.InterfaceImpls;
using Microsoft.AspNetCore.Mvc;

namespace Company.Controllers
{
    public class AccountController : Controller
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IEmployeeRepository<Employee> _employeeRepository;

        public AccountController(ICustomerRepository customerRepository, IEmployeeRepository<Employee> employeeRepository)
        {
            _customerRepository = customerRepository;
            _employeeRepository = employeeRepository;
        }

        public IActionResult Index(bool showBanned = true)
        {
            return View(new AccountListModel
            {
                Customers = _customerRepository.QueryFiltered(cus => showBanned || !cus.IsSoftDeleted),
                Employees = _employeeRepository.QueryFiltered(emp => showBanned || !emp.IsSoftDeleted),
                ShowBanned = showBanned
            });
        }
    }
}
