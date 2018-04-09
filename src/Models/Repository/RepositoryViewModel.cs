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

    public class RepositoryViewModel
    {
        public RepositoryViewModel(
            int commitCountInBranch,
            int branchCount,
            int releaseCount,
            string currentPath,
            Branch currentBranch,
            IEnumerable<string> parentDirectories,
            IEnumerable<string> branches,
            IEnumerable<RepoListEntryViewModel> tableEntries)
        {
            CommitCountInBranch = commitCountInBranch;
            BranchCount = branchCount;
            ReleaseCount = releaseCount;
            CurrentPath = currentPath;
            ParentDirectories = parentDirectories;
            Branches = branches;
            TableEntries = tableEntries;
            CurrentBranch = currentBranch;
        }

        public int CommitCountInBranch { get;  }
        public int BranchCount { get;  }
        public int ReleaseCount { get;  }

        // TODO: add custom attribute to validate the path
        // The name of current directory / file (e.g., the last level in directory tree)
        // the full path can be reconstructed using ParentDirectories

        public string CurrentPath { get;  }
        public Branch CurrentBranch { get;  }
        public IEnumerable<string> ParentDirectories { get;  }
        public IEnumerable<string> Branches { get;  }
        public IEnumerable<RepoListEntryViewModel> TableEntries { get;  }

    }


    public class RepoListEntryViewModel
    {
        public RepoListEntryViewModel(
            EntryType entryType, 
            string friendlyName, 
            string relativePath, 
            Commit latestCommit)
        {
            EntryType = entryType;
            FriendlyName = friendlyName;
            RelativePath = relativePath;
            LatestCommit = latestCommit;
        }

        public EntryType EntryType { get;  }
        public string FriendlyName { get;  }
        public string RelativePath { get;  }
        public Commit LatestCommit { get;  }
    }

    public enum EntryType { File, Directory }
}
