using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LibGit2Sharp;

namespace BitHub.Models.Repository
{
    public class BranchViewModel
    {
        public BranchViewModel(Branch currentBranch, IEnumerable<string> branches)
        {
            CurrentBranch = currentBranch;
            Branches = branches;
        }

        public Branch CurrentBranch { get; }
        public IEnumerable<string> Branches { get; }
    }
}
