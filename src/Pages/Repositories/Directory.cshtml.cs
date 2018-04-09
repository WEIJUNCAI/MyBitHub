using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using BitHub.Data;
using BitHub.Services;
using BitHub.Helpers.Repository;
using BitHub.Models.Repository;
using BitHub.Extensions;
using LibGit2Sharp;

namespace BitHub.Pages.Repositories
{
    public class DirectoryModel : PageModelBase
    {

        private readonly ApplicationDbContext _appDbContext;
        private readonly ILogger<IndexModel> _logger;
        private readonly IDirectoryManager _directoryManager;
        // need to create the Repository object in handler using the repo path
        // and the Repository class does not support configuration after creation
        // does not know how to do this in a built in container
        // so we create it manually and register it for disposal

        public DirectoryModel(
            ApplicationDbContext applicationDbContext,
            ILogger<IndexModel> logger,
            IDirectoryManager directoryManager)
        {
            _appDbContext = applicationDbContext;
            _logger = logger;
            _directoryManager = directoryManager;
        }


        public async Task<IActionResult> OnGetAsync(
            string owner, string reponame, string branch, string requestdir)
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
            string decodedRequestDir = WebUtility.UrlDecode(requestdir);
            string reqestDirFullPath = Path.Combine(RepoInfo.RootPath, decodedRequestDir);

            try
            {
                InitializeRepositoryObj(RepoInfo.RootPath);

                if (_repository.Head.FriendlyName != decodedBranch)
                    _repository.CheckoutBranch(decodedBranch);

                IEnumerable<string> dirs, files;
                _directoryManager.GetRelativeDirsAndFiles(RepoInfo.RootPath, out dirs, out files);

                InitHeaderSpecVM();
                InitBranchVM();
                InitPathVM(decodedRequestDir);
                InitTableVM(RepoInfo.RootPath, dirs, files);

            }
            catch(Exception ex)
            {
                _logger.LogError($"Fatal error occured trying to retrieve info for repository \"{owner}/{reponame}\", exception:\n{ex.Message}");
                return NotFound();
            }
            return Page();
        }


        /////////////////////////////////////////////////////////////////////
        ///                                                               ///
        ///                       Helper Methods                          ///
        ///                                                               ///
        /////////////////////////////////////////////////////////////////////          

    }

}