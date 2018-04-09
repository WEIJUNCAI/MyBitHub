using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BitHub.Authorizations.Requirements;
using BitHub.Data;
using Microsoft.AspNetCore.Mvc;

namespace BitHub.Authorizations.Handlers
{
    public class RepositoryOwnerHandler : AuthorizationHandler<RepositoryOwnerRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RepositoryOwnerHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RepositoryOwnerRequirement requirement)
        {
            if (!requirement.RequireAsOwner)
                context.Succeed(requirement);
            else if (context.Resource is ActionContext mvcContext)
            {
                if (mvcContext.RouteData.Values["Owner"] is string owner)
                {
                    if (_userManager.GetUserName(context.User) == owner)
                        context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }
    }
}
