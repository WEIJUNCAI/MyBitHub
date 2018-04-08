using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BitHub.Authorizations.Requirements
{
    public class RepositoryOwnerRequirement : IAuthorizationRequirement
    {
        public bool RequireAsOwner { get; }

        public RepositoryOwnerRequirement(bool requireAsOwner)
        {
            RequireAsOwner = requireAsOwner;
        }
    }
}
