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
        public int CommitCountInBranch { get; set; }

        public int BranchCount { get; set; }

        public int ReleaseCount { get; set; }

        // TODO: add custom attribute to validate the path
        // The name of current directory / file (e.g., the last level in directory tree)
        // the full path can be reconstructed using ParentDirectories

        public string CurrentPath { get; set; }

        public Branch CurrentBranch { get; set; }

        public IEnumerable<string> ParentDirectories { get; set; }

        public IEnumerable<string> Branches { get; set; }

        public IEnumerable<RepoListEntryViewModel> TableEntries { get; set; }

    }


    public class RepoListEntryViewModel
    {
        public EntryType EntryType { get; set; }
        public string FriendlyPath { get; set; }
        public Commit LatestCommit { get; set; }
    }

    public enum EntryType { File, Directory }
}
