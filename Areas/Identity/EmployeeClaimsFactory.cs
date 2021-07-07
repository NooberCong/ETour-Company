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
    public class EmployeeClaimsFactory : UserClaimsPrincipalFactory<Employee, Role>
    {
        public EmployeeClaimsFactory(
        UserManager<Employee> userManager,
        RoleManager<Role> roleManager,
        IOptions<IdentityOptions> options
        ) : base(userManager, roleManager, options)
        {
        }

        protected async override Task<ClaimsIdentity> GenerateClaimsAsync(Employee employee)
        {
            var identity = await base.GenerateClaimsAsync(employee);
            if (employee.IsSoftDeleted)
            {
                identity.AddClaim(new Claim("Banned", ""));
            }

            var isAdmin = await UserManager.IsInRoleAsync(employee, "Admin");
            
            if (isAdmin)
            {
                identity.AddClaim(new Claim("IsAdmin", ""));
            }

            return identity;
        }
    }
}
