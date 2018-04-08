using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

using BitHub.Authorizations.Requirements;
using BitHub.Data;

namespace BitHub.Authorizations.Handlers
{
    public class SignInHandler : AuthorizationHandler<SignInRequirement>
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public SignInHandler(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, SignInRequirement requirement)
        {
            if (!requirement.RequireSignedIn ||  _signInManager.IsSignedIn(context.User))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
