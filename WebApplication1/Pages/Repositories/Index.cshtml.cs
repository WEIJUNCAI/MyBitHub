using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using BitHub.Data;
using BitHub.Models.Repository;
using BitHub.Extensions;
using BitHub.Services;
using LibGit2Sharp;

namespace BitHub.Pages.Repositories
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly ILogger<IndexModel> _logger;
        private readonly IDirectoryManager _directoryManager;
        // need to create the Repository object in handler using the repo path
        // and the Repository class does not support configuration after creation
        // does not know how to do this in a built in container
        // so we create it manually and register it for disposal

        private Repository _repository;


        //[BindProperty]
        public RepositoryInfoModel RepoInfo { get; set; }

        //[BindProperty]
        public RepositoryViewModel RepoInfo_Additional { get; set; }


        public IndexModel(
            ApplicationDbContext applicationDbContext,
            ILogger<IndexModel> logger,
            IDirectoryManager directoryManager)
        {
            _appDbContext = applicationDbContext;
            _logger = logger;
            _directoryManager = directoryManager;
            // initialize the supplementary repo view model
            RepoInfo_Additional = new RepositoryViewModel();
        }


        public async Task<IActionResult> OnGetAsync(string owner, string reponame)
        {
            ConfigureLoadNavigationProperties();

            // Retrieve the requested repo informantion from DB
            RepoInfo = await _appDbContext.Repositories.FirstOrDefaultAsync(
                repo => repo.Owner == owner && repo.RepoName == reponame);

            if (RepoInfo == null || !InitializeRepositoryObj() || !GetAdditionalRepoVM())
            {
                _logger.LogError($"\nFailed retrieving repository informantion for repository \"{owner}/{reponame}\"");
                return NotFound();
            }

            IEnumerable<string> dirs, files;
            if (!GetRelativeDirsAndFiles(RepoInfo.RootPath, out dirs, out files))
            {
                _logger.LogError($"\nFailed retrieving file and directory info under path {RepoInfo.RootPath}");
                return NotFound();
            }

            RepoInfo_Additional.TableEntries = GetTableEntries(dirs, files);

            return Page();
        }


        /////////////////////////////////////////////////////////////////////
        ///                                                               ///
        ///                     Helper Methods                            ///
        ///                                                               ///
        /////////////////////////////////////////////////////////////////////

        private void ConfigureLoadNavigationProperties()
        {
            // By default, navigation properties are null, they are not loaded by default.  
            // For loading navigation property, 
            // we use “include” method of IQuearable for Eager loading.
            _appDbContext.Repositories.Include<RepositoryInfoModel, ICollection<RepoTagmentModel>>
                (repoInfo => repoInfo.Tags).ToArray();

            _appDbContext.Tagments.Include<RepoTagmentModel, RepoTagModel>
                (tagment => tagment.Tag).ToArray();
        }

        // initialize the libgit2sharp repository object for later git opeartions
        private bool InitializeRepositoryObj()
        {
            try
            {
                _repository = new Repository(RepoInfo.RootPath);

                // register for disposal for the repository object after fulfilling the request
                // it seems we must call register AFTER the object has been initialized.
                HttpContext.Response.RegisterForDispose(_repository);
            }
            catch (Exception ex)
            {
                _logger.LogError($"\nFailed initializing repository object on path\"{RepoInfo.RootPath}\", error message: {ex.Message}");
                return false;
            }

            return true;
        }

        // fill in the additional repository view model using repository info
        private bool GetAdditionalRepoVM()
        {
            try
            {
                RepoInfo_Additional.CurrentBranch = _repository.Head;
                RepoInfo_Additional.BranchCount = _repository.GetBranchCount();
                RepoInfo_Additional.CommitCountInBranch = _repository.Head.GetBranchCommitCount();
                RepoInfo_Additional.CurrentPath = string.Empty;
                RepoInfo_Additional.Branches = _repository.Branches.Select(branch => branch.FriendlyName).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError($"\nFailed retrieving git repository info, error message: {ex.Message}");
                return false;
            }

            return true;
        }


        // 
        private IEnumerable<RepoListEntryViewModel> GetTableEntries(IEnumerable<string> dirs, IEnumerable<string> files)
        {
            var tableEntries = new List<RepoListEntryViewModel>();

            var allPaths = dirs.Concat(files);

            var commits = GetLatestCommits(RepoInfo.RootPath, allPaths).GetEnumerator();
            commits.MoveNext();

            foreach (string dir in dirs)
            {
                tableEntries.Add(new RepoListEntryViewModel
                {
                    EntryType = EntryType.Directory,
                    RelativePath = dir,
                    FriendlyName = Path.GetFileName(dir),
                    LatestCommit = commits.Current
                }
                );
                commits.MoveNext();
            }
            foreach (string file in files)
            {
                tableEntries.Add(new RepoListEntryViewModel
                {
                    EntryType = EntryType.File,
                    RelativePath = file,
                    FriendlyName = Path.GetFileName(file),
                    LatestCommit = commits.Current
                }
                );
                commits.MoveNext();
            }

            return tableEntries;
        }

        
        private bool GetDirsAndFiles(
            string targetDir, out IEnumerable<string> dirs, out IEnumerable<string> files)
        {
            try
            {
                dirs = _directoryManager.GetDirectories(RepoInfo.RootPath);
                files = _directoryManager.GetFiles(RepoInfo.RootPath);
            }
            catch (Exception ex)
            {
                dirs = new string[0];
                files = new string[0];
                _logger.LogError($"\nException thrown:{ex.Message}");
                return false;
            }
            return true;
        }

        // get all relative directories and files under repository root
        // will skip dirs and files that start with ".", which is assumed to be hidden
        private bool GetRelativeDirsAndFiles(
            string targetDir, out IEnumerable<string> relativeDirs, out IEnumerable<string> relativeFiles)
        {
            IEnumerable<string> fullPathDirs, fullPathFiles;
            if (!GetDirsAndFiles(targetDir, out fullPathDirs, out fullPathFiles))
            {
                relativeDirs = new string[0];
                relativeFiles = new string[0];
                return false;
            }

            relativeDirs = fullPathDirs
                 .Select(dir => GetRelativePath(RepoInfo.RootPath, dir))
                 .Where(dir => !dir.StartsWith('.')).ToArray();

            relativeFiles = fullPathFiles
                 .Select(file => GetRelativePath(RepoInfo.RootPath, file))
                 .Where(file => !file.StartsWith('.')).ToArray();

            return true;

        }

        // get the latest commit associated with a folder or file
        //  should have been implemented with libgit2sharp, but
        //  the "QueryBy(file)" method apparently has serious bug
        //  so we have to use command line git
        private IEnumerable<Commit> GetLatestCommits(string repoRootPath, IEnumerable<string> targetRelativePaths)
        {
            List<Commit> commits = new List<Commit>();
            try
            {
                var shas = GetLatestCommitShas(repoRootPath, targetRelativePaths);

                foreach(string sha in shas)
                   commits.Add((sha.Length == 0) ? null : _repository.Lookup<Commit>(sha));

                return commits;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed retrieving latest commit from command line, error:\"{ex.Message}\"");
            }
            return null;
        }

        // helper method that gets the SHA of the desired latest commit associated with a file or folder
        // use command line git and parse the output
        private IEnumerable<string> GetLatestCommitShas(string repoRootPath, IEnumerable<string> targetRelativePaths)
        {
            List<string> commitShas = new List<string>();

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                WorkingDirectory = repoRootPath
            };

            process.StartInfo = startInfo;
            process.Start();

            foreach (string path in targetRelativePaths)
            {
                string command = $"git log -n 1 --follow \"{path}\"\r\n";
                process.StandardInput.Write(command);

                while (true)
                {
                    string line = process.StandardOutput.ReadLine();
                    if (line.IndexOf(command.Substring(0, command.Length - 2)) != -1)
                    {
                        string gitOutputLine = process.StandardOutput.ReadLine();
                        int index1 = gitOutputLine.IndexOf(' ');
                        if (index1 != -1)
                            gitOutputLine = gitOutputLine.Substring(index1 + 1, gitOutputLine.Length - index1 - 1);
                        commitShas.Add(gitOutputLine);
                        break;
                    }
                }
            }
            process.Close();

            return commitShas;
        }

        private string GetRelativePath(string roottFullPath, string childFullPath)
        {
            if (!childFullPath.StartsWith(roottFullPath))
                return null;

            return childFullPath.Substring(roottFullPath.Length + 1);
        }

        // Helper method calculate the elapsed time for files and commits
        // elapsed time expressed in days, hours, min
        // TODO: reimplement to express elapsed time in month and years
        public string GetTimeDifference(DateTimeOffset timeStamp)
        {
            TimeSpan elapsedTime = DateTimeOffset.UtcNow - timeStamp;
            StringBuilder sb = new StringBuilder();

            if (elapsedTime.Days != 0)
                sb.Append(elapsedTime.Days).Append((elapsedTime.Days != 1) ? " days" : " day");
            else if (elapsedTime.Hours != 0)
                sb.Append(elapsedTime).Append((elapsedTime.Hours != 1) ? " hours" : " hour");
            else if (elapsedTime.Minutes != 0)
                sb.Append(elapsedTime.Minutes).Append(elapsedTime.Minutes != 1 ? " minutes" : " minute");
            else
                sb.Append(elapsedTime.Seconds).Append(elapsedTime.Seconds > 1 ? " seconds" : " second");

            sb.Append(" ago");
            return sb.ToString();
        }
    }
}