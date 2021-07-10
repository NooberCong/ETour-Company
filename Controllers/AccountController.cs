using Company.Models;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.InterfaceImpls;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ITourReviewRepository _tourReviewRepository;
        private readonly IEmployeeRepository<Employee> _employeeRepository;
        private readonly IETourLogger _eTourLogger;
        private readonly IUnitOfWork _unitOfWork;

        public AccountController(ICustomerRepository customerRepository, ITourReviewRepository tourReviewRepository, IBookingRepository bookingRepository, IEmployeeRepository<Employee> employeeRepository, IUnitOfWork unitOfWork, IETourLogger eTourLogger)
        {
            _customerRepository = customerRepository;
            _employeeRepository = employeeRepository;
            _tourReviewRepository = tourReviewRepository;
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
            _eTourLogger = eTourLogger;
        }

        public IActionResult Index(bool showBanned = true)
        {
            var emps = _employeeRepository.QueryFiltered(emp => showBanned || !emp.IsSoftDeleted);

            foreach (var employee in emps)
            {
                System.Console.WriteLine(employee.FullName);
                foreach (var role in ((IEmployee)employee).Roles)
                {
                    System.Console.WriteLine(role.Name);
                }
                System.Console.WriteLine("--------");
            }

            return View(new AccountListModel
            {
                Customers = _customerRepository.Queryable.Where(cus => showBanned || !cus.IsSoftDeleted).AsEnumerable(),
                Employees = emps,
                ShowBanned = showBanned
            });
        }

        public async Task<IActionResult> DetailCustomer(string id)
        {
            var customer = await _customerRepository.Queryable
                .Include(c => c.Bookings)
                .ThenInclude(bk => bk.CustomerInfos)
                .Include(c => c.PointLogs)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (customer == null)
            {
                return NotFound();
            }

            customer.Bookings = await _bookingRepository.Queryable
                .Where(bk => bk.OwnerID == customer.ID)
                .Include(bk => bk.Trip)
                .ThenInclude(tr => tr.Tour)
                .Include(bk => bk.CustomerInfos)
                .ToListAsync();


            customer.Reviews.AddRange(_tourReviewRepository.GetReviewsForCustomer(customer));

            return View(customer);
        }

        public async Task<IActionResult> DetailEmployee(string id)
        {
            var employee = await _employeeRepository.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        public async Task<IActionResult> EditEmployee(string id)
        {
            var employee = await _employeeRepository.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            var roles = _employeeRepository.GetAllRoles();

            return View(new EmployeeFormModel
            {
                Employee = employee,
                Roles = roles
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditEmployee(Employee employee, string[] roles, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            var existingEmployee = await _employeeRepository.FindAsync(employee.ID);
            if (existingEmployee == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(new EmployeeFormModel
                {
                    Employee = employee,
                    Roles = _employeeRepository.GetAllRoles()
                });
            }

            await _employeeRepository.UpdateAsync(employee, roles);
            await _eTourLogger.LogAsync(Log.LogType.Modification, $"{User.Identity.Name} updated employee account {employee.Email}");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = "Employee updated successfully";
            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> ToggleBanCustomer(string id, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            var customer = await _customerRepository.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            customer.IsSoftDeleted = !customer.IsSoftDeleted;
            await _customerRepository.UpdateAsync(customer);
            await _eTourLogger.LogAsync(customer.IsSoftDeleted? Log.LogType.Deletion: Log.LogType.Creation, $"{User.Identity.Name} {(customer.IsSoftDeleted ? "banned" : "unbanned")} customer {customer.Name}");
            await _unitOfWork.CommitAsync();

            TempData["StatusMessage"] = $"User {customer.Name} {(customer.IsSoftDeleted ? "banned" : "unbanned")} successfully";

            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> ToggleBanEmployee(string id, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");

            var employee = await _employeeRepository.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            if (!((IEmployee)employee).IsAdmin())
            {
                employee.IsSoftDeleted = !employee.IsSoftDeleted;
                await _employeeRepository.UpdateAsync(employee);
                await _eTourLogger.LogAsync(employee.IsSoftDeleted ? Log.LogType.Deletion : Log.LogType.Creation, $"{User.Identity.Name} {(employee.IsSoftDeleted ? "banned" : "unbanned")} employee account {employee.Email}");
                await _unitOfWork.CommitAsync();
                TempData["StatusMessage"] = $"Employee {employee.UserName} {(employee.IsSoftDeleted ? "banned" : "unbanned")} successfully";
            }
            else
            {
                TempData["StatusMessage"] = $"Error: Cannot perform ban action on an Admin account";
            }

            return LocalRedirect(returnUrl);
        }
    }
}
