using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using BitHub.Data;
using BitHub.Services;
using BitHub.Models.Repository;
using BitHub.Extensions;
using LibGit2Sharp;

namespace BitHub.Pages.Repositories
{
    public class FileModel : PageModel
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly ILogger<IndexModel> _logger;
        private readonly IFileManager _fileManager;
        private readonly IFileInfoManager _fileInfoManager;
        private Repository _repository;

        public RepositoryInfoModel RepoInfo { get; set; }

        public RepositoryViewModel RepoInfo_Additional { get; set; }

        public FileViewModel FileInfo { get; set; }


        public FileModel(
            ApplicationDbContext appDbContext,
            ILogger<IndexModel> logger,
            IFileManager fileManager,
            IFileInfoManager fileInfoManager)
        {
            _appDbContext = appDbContext;
            _logger = logger;
            _fileManager = fileManager;
            _fileInfoManager = fileInfoManager;
        }

        public async Task<IActionResult> OnGetAsync(
            string owner, string reponame, string branch, string requestfile)
        {
            // Retrieve the requested repo informantion from DB
            RepoInfo = await _appDbContext.Repositories.FirstOrDefaultAsync(
                repo => repo.Owner == owner && repo.RepoName == reponame);

            if (RepoInfo == null || !InitializeRepositoryObj())
            {
                _logger.LogError($"\nFailed retrieving repository informantion for repository \"{owner}/{reponame}\"");
                return NotFound();
            }

            // The "branch" and "RequestDir" parameters might contain '/' character which is percent encoded in URL,
            // however they are not decoded in model binding, we have to manually decode them

            string decodedBranch = WebUtility.UrlDecode(branch);
            string decodedRequestFile = WebUtility.UrlDecode(requestfile);
            string reqestFileFullPath = Path.Combine(RepoInfo.RootPath, decodedRequestFile);

            if (_repository.Head.FriendlyName != decodedBranch && !CheckoutBranch(decodedBranch))
            {
                _logger.LogError($"\nFailed checking out branch {decodedBranch}.");
                return NotFound();
            }

            if (!GetAdditionalRepoVM(decodedRequestFile))
            {
                _logger.LogError($"\nFailed retrieving additional repository informantion for repository \"{owner}/{reponame}\"");
                return NotFound();
            }

            GetFileVM(RepoInfo.RootPath, decodedRequestFile);

            return Page();
        }



        /////////////////////////////////////////////////////////////////////
        ///                                                               ///
        ///                       Helper Methods                          ///
        ///                                                               ///
        /////////////////////////////////////////////////////////////////////


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
        private bool GetAdditionalRepoVM(string requestDir)
        {
            try
            {
                var splitedDirs = SplitDir(requestDir);

                RepoInfo_Additional = new RepositoryViewModel(
                    currentBranch: _repository.Head,
                    branchCount: _repository.GetBranchCount(),
                    releaseCount: 0,
                    commitCountInBranch: _repository.Head.GetBranchCommitCount(),
                    currentPath: splitedDirs.Item2,
                    parentDirectories: splitedDirs.Item1,
                    branches: _repository.Branches.Select(branch => branch.FriendlyName).ToArray(),
                    tableEntries: null
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"\nFailed retrieving git repository info, error message: {ex.Message}");
                return false;
            }

            return true;
        }

        private bool GetFileVM(string repoRootPath, string relativeFilePath)
        {
            string fullFilePath = Path.Combine(repoRootPath, relativeFilePath);
            StringBuilder sb = new StringBuilder();
            uint lineCount = 0;
            using (StreamReader reader = new StreamReader(_fileManager.Open(fullFilePath, FileMode.Open), Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    ++lineCount;
                    sb.AppendLine(/*WebUtility.HtmlEncode*/(line));
                }
            }

            _fileInfoManager.SetFilePath(fullFilePath);
            FileInfo = new FileViewModel(
                fullPath: relativeFilePath,
                content: sb.ToString(),
                lineCount: lineCount,
                size: _fileInfoManager.GetLength(),
                languageShort: Path.GetExtension(relativeFilePath).Substring(1),
                languageFull: null,
                latestCommt: GetLatestCommits(RepoInfo.RootPath, new string[] {relativeFilePath}, false).First()
            );
            return true;
        }


        private bool CheckoutBranch(string branch)
        {
            try
            {
                Branch targetBranch = _repository.Branches[branch];
                if (targetBranch == null)
                {
                    _logger.LogError($"The branch \"{targetBranch}\" does not exist.");
                    return false;
                }
                Commands.Checkout(_repository, targetBranch);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown: \"{ex.Message}\"");
            }
            return false;
        }


        private IEnumerable<Commit> GetLatestCommits(string repoRootPath, IEnumerable<string> targetRelativePaths, bool includeRename)
        {
            List<Commit> commits = new List<Commit>();
            try
            {
                var shas = GetLatestCommitShas(repoRootPath, targetRelativePaths, includeRename);

                foreach (string sha in shas)
                    commits.Add((sha.Length == 0) ? null : _repository.Lookup<Commit>(sha));

                return commits;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed retrieving latest commit from command line, error:\"{ex.Message}\"");
            }
            return null;
        }

        // helper method that gets the SHAs of the desired latest commit associated with all files and folders
        // we provide it with all target paths to avoid the overhead of creating a new porocess
        // for each path.
        // use command line git and parse the output
        // note the use of ReadLine or ReadToEnd might block indefinitely since we do not provide starting arguments
        // and in this case the output stream does not have an end

        // we will sure find the command that has been echoed back after executing it
        // and on the next line, the git log result.
        // if it is empty, no commit record for this path; otheriwse we get the commit SHA
        // *** Note *** 
        // although very unlikely, the description might also contain the same command
        // which could lead to misinterpretation.


        // in the directory entries, the latest commit includes rename history,
        // which means we should use "--follow" option when rendering directory page;
        // file page, however, does not go beyond rename.

        private IEnumerable<string> GetLatestCommitShas(string repoRootPath, IEnumerable<string> targetRelativePaths, bool includeRename)
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
            string renameSwitch = includeRename ? "--follow" : string.Empty;
            foreach (string path in targetRelativePaths)
            {
                string command = $"git log -n 1 {renameSwitch} \"{path}\"\r\n";
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


        // split the directory into individual levels
        // returns the parent levels and the last level
        private Tuple<IEnumerable<string>, string> SplitDir(string dir)
        {
            string[] allLevels = dir.Split('\\');

            string lastLevel = allLevels.Last();
            IEnumerable<string> parentLevels = allLevels.Take(allLevels.Length - 1);
            return new Tuple<IEnumerable<string>, string>(parentLevels, lastLevel);
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

        public string ReconstructPath(string lastLevel)
        {
            string result = string.Empty;
            foreach (string dir in RepoInfo_Additional.ParentDirectories)
            {
                result = Path.Combine(result, dir);
                if (dir == lastLevel)
                    break;
            }
            return result;
        }
    }
}