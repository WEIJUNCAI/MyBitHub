using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LibGit2Sharp;

namespace BitHub.Models.Repository
{
    public class TableViewModel
    {
        public TableViewModel(IEnumerable<RepoListEntryViewModel> tableEntries)
        {
            TableEntries = tableEntries;
        }

        public IEnumerable<RepoListEntryViewModel> TableEntries { get; }
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

        public EntryType EntryType { get; }
        public string FriendlyName { get; }
        public string RelativePath { get; }
        public Commit LatestCommit { get; }
    }

    public enum EntryType { File, Directory }
}
