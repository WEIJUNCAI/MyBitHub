using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LibGit2Sharp;

namespace BitHub.Models.Repository
{
    // The view model of a repository on Repositories/Index page
    // currently does NOT contain all properties like owner, description etc.
    // but rather serves as a wrapper for additional view properties
    // that complements the RepositoryInfoModel

    public class HeaderSpecViewModel
    {
        public HeaderSpecViewModel(
            int commitCountInBranch,
            int branchCount,
            int releaseCount)
        {
            CommitCountInBranch = commitCountInBranch;
            BranchCount = branchCount;
            ReleaseCount = releaseCount;
        }

        public int CommitCountInBranch { get;  }
        public int BranchCount { get;  }
        public int ReleaseCount { get;  }
    }
}
