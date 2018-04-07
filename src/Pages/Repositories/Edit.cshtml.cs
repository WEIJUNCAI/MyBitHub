using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

using BitHub.Data;
using BitHub.Services;
using BitHub.Models.Repository;
using BitHub.Extensions;
using LibGit2Sharp;

namespace BitHub.Pages.Repositories
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EditModel> _logger;
        private readonly IFileManager _fileManager;
        private readonly IFileInfoManager _fileInfoManager;
        private Repository _repository;

        public RepositoryInfoModel RepoInfo { get; set; }

        public RepositoryViewModel RepoInfo_Additional { get; set; }

        public FileViewModel FileInfo { get; set; }

        public RadioSelection[] Radios { get; set; }

        [BindProperty]
        public EditInputModel Input { get; set; }

        public EditModel(
            ApplicationDbContext appDbContext,
            UserManager<ApplicationUser> userManager,
            ILogger<EditModel> logger,
            IFileManager fileManager,
            IFileInfoManager fileInfoManager)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
            _logger = logger;
            _fileManager = fileManager;
            _fileInfoManager = fileInfoManager;

            // initialize the supplementary repo view model
            RepoInfo_Additional = new RepositoryViewModel();
            FileInfo = new FileViewModel();
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

            ConfigureRadioBtn();

            return Page();
        }


        public async Task<IActionResult> OnPostAsync(
            string owner, string reponame, string branch, string requestfile)
        {
            if (!ModelState.IsValid)
                return Page();

            // Retrieve the requested repo informantion from DB
            RepoInfo = await _appDbContext.Repositories.FirstOrDefaultAsync(
                repo => repo.Owner == owner && repo.RepoName == reponame);

            if (RepoInfo == null || !InitializeRepositoryObj())
            {
                _logger.LogError($"\nFailed retrieving repository informantion for repository \"{owner}/{reponame}\"");
                return NotFound();
            }

            // The "branch" and "RequestDir" parameters might contain '\' character which is percent encoded in URL,
            // however they are not decoded in model binding, we have to manually decode them

            string decodedRequestFile = WebUtility.UrlDecode(requestfile);

            if (!await CommitChangesAsync(decodedRequestFile))
            {
                _logger.LogError($"\nFailed commiting changes to repository \"{owner}/{reponame}\"");
                return NotFound();
            }

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

                RepoInfo_Additional.CurrentBranch = _repository.Head;
                RepoInfo_Additional.BranchCount = _repository.GetBranchCount();
                RepoInfo_Additional.CommitCountInBranch = _repository.Head.GetBranchCommitCount();
                RepoInfo_Additional.CurrentPath = splitedDirs.Item2;
                RepoInfo_Additional.ParentDirectories = splitedDirs.Item1;
                RepoInfo_Additional.Branches = _repository.Branches.Select(branch => branch.FriendlyName).ToArray();
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
            FileInfo.FullPath = relativeFilePath;
            FileInfo.Content = sb.ToString();
            FileInfo.LineCount = lineCount;
            FileInfo.Size = _fileInfoManager.GetLength();
            FileInfo.LanguageShort = Path.GetExtension(relativeFilePath).Substring(1);
            //FileInfo.LatestCommt = GetLatestCommits(RepoInfo.RootPath, new string[] { relativeFilePath }, false).First();
            return true;
        }

        private void ConfigureRadioBtn()
        {
            Radios = new RadioSelection[]
            {
                new RadioSelection{ Id = 0 },
                new RadioSelection{ Id = 1 }
            };
        }

        private async Task<bool> CommitChangesAsync(string relativeFilePath)
        {
            try
            {
                // create a new branch before commit if specified by user
                if (Input.CreateNewBranch == 1)
                {
                    _repository.CreateBranch(Input.NewBranchName);
                    CheckoutBranch(Input.NewBranchName);
                }

                string fullFilePath = Path.Combine(RepoInfo.RootPath, relativeFilePath);
                _fileManager.WriteAllText(fullFilePath, Input.Content);

                // Stage the file
                _repository.Index.Add(relativeFilePath);

                // retrives the info of current signed in user
                var appUser = await _userManager.GetUserAsync(User);

                // setting up commit signiture
                Signature author = new Signature(appUser.UserName, appUser.Email, DateTime.Now);
                Signature committer = author;

                // Commit to the repository
                Commit commit = _repository.Commit($"{Input.Title}\n{Input.Description}", author, committer);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown: \"{ex.Message}\"");
                return false;
            }
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


        // should use number to match level instead of name, since
        // different level of dirs can have the same name.
        public string ReconstructPath(int lastLevel)
        {
            string result = string.Empty;
            int curLevel = 0;
            foreach (string dir in RepoInfo_Additional.ParentDirectories)
            {
                result = Path.Combine(result, dir);
                if (curLevel++ == lastLevel)
                    break;
            }

            // if true, the desired last level is the current level
            if (curLevel == lastLevel)
                result = Path.Combine(result, RepoInfo_Additional.CurrentPath);

            return result;
        }


        // the requested repo directory path is separated by Windows style '\'
        // which will be URL encoded
        // might be helpful for routing to distinguish between the actuall route 
        // requested dir or file path

        private Tuple<IEnumerable<string>, string> SplitDir(string dir)
        {
            string[] allLevels = dir.Split('\\');

            string lastLevel = allLevels.Last();
            IEnumerable<string> parentLevels = allLevels.Take(allLevels.Length - 1);
            return new Tuple<IEnumerable<string>, string>(parentLevels, lastLevel);
        }
    }
}