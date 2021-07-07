using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Company.AuthorizationHandlers
{
    public class TripAuthorizationHandler : AuthorizationHandler<OwnedTripRequirement, Trip>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnedTripRequirement requirement, Trip resource)
        {
            if (context.User.HasClaim(cl => cl.Type == "IsAdmin") ||
                resource.OwnerID == context.User.Claims.FirstOrDefault(cl => cl.Type == ClaimTypes.NameIdentifier)?.Value)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    public class OwnedTripRequirement : IAuthorizationRequirement { }
}
