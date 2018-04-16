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
using BitHub.Helpers.Repository;
using BitHub.Services;
using LibGit2Sharp;
using System.Net;

namespace BitHub.Pages.Repositories
{
    public class IndexModel : PageModelBase
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly ILogger<IndexModel> _logger;
        private readonly IDirectoryManager _directoryManager;
        private readonly IFileManager _fileManager;
        // need to create the Repository object in handler using the repo path
        // and the Repository class does not support configuration after creation
        // does not know how to do this in a built in container
        // so we create it manually and register it for disposal

        public IndexModel(
            ApplicationDbContext applicationDbContext,
            ILogger<IndexModel> logger,
            IDirectoryManager directoryManager,
            IFileManager fileManager)
        {
            _appDbContext = applicationDbContext;
            _logger = logger;
            _directoryManager = directoryManager;
            _fileManager = fileManager;
        }

        public string ReadmeContent { get; private set; }

        public async Task<IActionResult> OnGetAsync(string owner, string reponame, string branch)
        {
            ConfigureLoadNavigationProperties();

            // Retrieve the requested repo informantion from DB
            RepoInfo = await _appDbContext.Repositories.FirstOrDefaultAsync(
                repo => repo.Owner == owner && repo.RepoName == reponame);

            if (RepoInfo == null)
            {
                _logger.LogError($"\nFailed retrieving repository informantion for repository \"{owner}/{reponame}\"");
                return NotFound();
            }

            try
            {
                InitializeRepositoryObj(RepoInfo.RootPath);

                // Branch switching is handled by repo home page
                // switch branch in any directory will bring back to home page
                if (branch != null)
                {
                    string decodedBranch = WebUtility.UrlDecode(branch);
                    if(_repository.Head.FriendlyName != decodedBranch)
                        _repository.CheckoutBranch(decodedBranch);
                }

                IEnumerable<string> dirs, files;
                _directoryManager.GetRelativeDirsAndFiles(RepoInfo.RootPath, RepoInfo.RootPath, out dirs, out files);
                InitHeaderSpecVM();
                InitBranchVM();
                InitTableVM(RepoInfo.RootPath, dirs, files);
                InitReadmeVM(RepoInfo.RootPath, files);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fatal error occured trying to retrieve info for repository \"{owner}/{reponame}\", exception:\n{ex.Message}\n {ex.StackTrace}");
                return NotFound();
            }

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


        private void InitReadmeVM(string rootPath, IEnumerable<string> relativeFilePaths)
        {
            string relativeReadmePath = relativeFilePaths.FirstOrDefault(
                file => Path.GetFileName(file) == "README.md");

            // no README.md, return
            if (relativeReadmePath == null)
                return;

            try
            {
                ReadmeContent = _fileManager.ReadAllText(Path.Combine(rootPath, relativeReadmePath));
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed reading the content of README.md", ex);
            }
        }

    }
}