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
using Microsoft.AspNetCore.Authorization;

using BitHub.Data;
using BitHub.Services;
using BitHub.Models.Repository;
using BitHub.Extensions;
using BitHub.Helpers.Repository;
using LibGit2Sharp;

namespace BitHub.Pages.Repositories
{

    [Authorize(policy: "SignedIn")]
    [Authorize(policy: "RepoOwner")]
    public class EditModel : PageModelBase
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EditModel> _logger;
        private readonly IFileManager _fileManager;
        private readonly IFileInfoManager _fileInfoManager;


        public RadioSelection[] Radios { get; set; }

        [BindProperty]
        public EditInputModel Input { get; set; }

        public EditModel(
            ApplicationDbContext appDbContext,
            IAuthorizationService authorizationService,
            UserManager<ApplicationUser> userManager,
            ILogger<EditModel> logger,
            IFileManager fileManager,
            IFileInfoManager fileInfoManager)
        {
            _appDbContext = appDbContext;
            _authorizationService = authorizationService;
            _userManager = userManager;
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

            if (RepoInfo == null)
            {
                _logger.LogError($"\nFailed retrieving repository informantion for repository \"{owner}/{reponame}\"");
                return NotFound();
            }

            // The "branch" and "RequestDir" parameters might contain '/' character which is percent encoded in URL,
            // however they are not decoded in model binding, we have to manually decode them

            string decodedBranch = WebUtility.UrlDecode(branch);
            string decodedRequestFile = WebUtility.UrlDecode(requestfile);
            string reqestFileFullPath = Path.Combine(RepoInfo.RootPath, decodedRequestFile);

            try
            {
                InitializeRepositoryObj(RepoInfo.RootPath);

                if (_repository.Head.FriendlyName != decodedBranch)
                    _repository.CheckoutBranch(decodedBranch);

                _fileInfoManager.SetFilePath(reqestFileFullPath);
                var fileRes = _fileManager.GetFileAllTextAndLineCount(reqestFileFullPath);
                _fileInfoManager.SetFilePath(reqestFileFullPath);
                long size = _fileInfoManager.GetLength();

                InitHeaderSpecVM();
                InitPathVM(decodedRequestFile);
                InitBranchVM();
                InitFileVM(RepoInfo.RootPath, decodedRequestFile, fileRes.Item2, size, fileRes.Item1);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fatal error occured trying to retrieve info for repository \"{owner}/{reponame}\", exception:\n{ex.Message}");
                return NotFound();
            }

            ConfigureRadioBtn();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string owner, string reponame, string branch, string requestfile)
        {
            if (!ModelState.IsValid)
                return Page();

            // Retrieve the requested repo informantion from DB
            RepoInfo = await _appDbContext.Repositories.FirstOrDefaultAsync(
                repo => repo.Owner == owner && repo.RepoName == reponame);

            if (RepoInfo == null)
            {
                _logger.LogError($"\nFailed retrieving repository informantion for repository \"{owner}/{reponame}\"");
                return NotFound();
            }

            // The "branch" and "RequestDir" parameters might contain '\' character which is percent encoded in URL,
            // however they are not decoded in model binding, we have to manually decode them

            string decodedRequestFile = WebUtility.UrlDecode(requestfile);

            InitializeRepositoryObj(RepoInfo.RootPath);

            if (!await CommitChangesAsync(decodedRequestFile))
            {
                _logger.LogError($"\nFailed commiting changes to repository \"{owner}/{reponame}\"");
                return NotFound();
            }

            return RedirectToPage("./File", this.RouteData);
        }



        /////////////////////////////////////////////////////////////////////
        ///                                                               ///
        ///                       Helper Methods                          ///
        ///                                                               ///
        /////////////////////////////////////////////////////////////////////



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
                    _repository.CheckoutBranch(Input.NewBranchName);
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



    }
}