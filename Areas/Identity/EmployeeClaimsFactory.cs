using Infrastructure.InterfaceImpls;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Company.Areas.Identity
{
    public class EmployeeClaimsFactory : UserClaimsPrincipalFactory<Employee>
    {
        public EmployeeClaimsFactory(UserManager<Employee> employeeManager,
       IOptions<IdentityOptions> optionsAccessor) : base(employeeManager, optionsAccessor)
        { }
        protected async override Task<ClaimsIdentity> GenerateClaimsAsync(Employee employee)
        {
            var identity = await base.GenerateClaimsAsync(employee);
            if (employee.IsSoftDeleted)
            {
                identity.AddClaim(new Claim("Banned", ""));
            }

            return identity;
        }
    }
}
