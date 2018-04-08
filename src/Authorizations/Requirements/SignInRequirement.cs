using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BitHub.Authorizations.Requirements
{
    public class SignInRequirement : IAuthorizationRequirement
    {
        public bool RequireSignedIn { get; }

        public SignInRequirement(bool reqireSignedIn)
        {
            RequireSignedIn = reqireSignedIn;
        }
    }
}
