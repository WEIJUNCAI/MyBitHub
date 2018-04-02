using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LibGit2Sharp;

namespace BitHub.Extensions
{
    public static class Libgit2Extensions
    {
        public static int GetBranchCount(this IRepository repo)
        {
            var branches = repo.Branches;
            int count = 0;

            foreach (var branch in branches)
                ++count;

            return count;
        }

        public static int GetBranchCommitCount(this Branch branch)
        {
            var commits = branch.Commits;

            int count = 0;

            foreach (var commit in commits)
                ++count;

            return count;
        }
    }
}
