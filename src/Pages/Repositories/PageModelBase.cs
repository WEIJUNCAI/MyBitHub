using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BitHub.Data;
using BitHub.Models.Repository;
using BitHub.Helpers.Repository;
using BitHub.Extensions;
using BitHub.Services;
using LibGit2Sharp;
using System.Text;

namespace BitHub.Pages.Repositories
{
    public abstract class PageModelBase : PageModel
    {
        protected Repository _repository;

        public RepositoryInfoModel RepoInfo { get; set; }
        public HeaderSpecViewModel HeaderSpecVM { get; set; }
        public PathViewModel PathVM { get; set; }
        public TableViewModel TableVM { get; set; }
        public BranchViewModel BranchVM { get; set; }
        public FileViewModel FileVM { get; set; }




        // initialize the libgit2sharp repository object for later git opeartions
        protected void InitializeRepositoryObj(string repoRootPath)
        {
            try
            {
                _repository = new Repository(repoRootPath);

                // register for disposal for the repository object after fulfilling the request
                // it seems we must call register AFTER the object has been initialized.
                HttpContext.Response.RegisterForDispose(_repository);
            }
            catch (Exception ex)
            {
                //_logger.LogError($"\nFailed initializing repository object on path\"{RepoInfo.RootPath}\", error message: {ex.Message}");
                throw new ApplicationException($"Failed initializing repository object on path\"{RepoInfo.RootPath}\".", ex);
            }
        }


        

        // fill in the additional repository view model using repository info
        protected void InitHeaderSpecVM()
        {
            try
            {
                HeaderSpecVM = new HeaderSpecViewModel(
                    branchCount: _repository.GetBranchCount(),
                    commitCountInBranch: _repository.Head.GetBranchCommitCount(),
                    releaseCount: 0);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed retrieving git repository info.", ex);
            }
        }

        protected void InitPathVM(string relativeRequestDir)
        {
            var splitedDirs = DirectoryHelper.SplitDir(relativeRequestDir);

            PathVM = new PathViewModel(
                parentDirectories: splitedDirs.Item1,
                currentPath: splitedDirs.Item2);

        }

        protected void InitBranchVM()
        {
            BranchVM = new BranchViewModel(
                currentBranch: _repository.Head,
                branches: _repository.Branches.Select(branch => branch.FriendlyName).ToArray());
        }

        protected void InitTableVM(
            string repoRootPath, IEnumerable<string> dirs, IEnumerable<string> files)
        {
            var tableEntries = new List<RepoListEntryViewModel>();

            try
            {
                var allPaths = dirs.Concat(files);
                var commits = _repository.GetLatestCommits(repoRootPath, allPaths, true).GetEnumerator();
                commits.MoveNext();

                foreach (string dir in dirs)
                {
                    tableEntries.Add(new RepoListEntryViewModel
                    (
                        entryType: EntryType.Directory,
                        relativePath: dir,
                        friendlyName: Path.GetFileName(dir),
                        latestCommit: commits.Current
                    ));
                    commits.MoveNext();
                }
                foreach (string file in files)
                {
                    tableEntries.Add(new RepoListEntryViewModel
                    (
                        entryType: EntryType.File,
                        relativePath: file,
                        friendlyName: Path.GetFileName(file),
                        latestCommit: commits.Current
                    ));
                    commits.MoveNext();
                }
            }
            catch(Exception ex)
            {
                throw new ApplicationException("Failed constructing repository directory table.", ex);
            }

            TableVM = new TableViewModel(tableEntries);
        }

        public void InitFileVM(
            string repoRootPath, string relativeFilePath, string content, long size, uint lines)
        {
            
            FileVM = new FileViewModel(
                fullPath: relativeFilePath,
                content: content,
                lineCount: lines,
                size: size,
                languageShort: Path.GetExtension(relativeFilePath).Substring(1),
                languageFull: null,
                latestCommt: _repository.GetLatestCommits(repoRootPath, new string[] { relativeFilePath }, false).First()
            );
        }
    }
}
