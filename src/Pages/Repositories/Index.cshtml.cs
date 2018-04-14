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

namespace BitHub.Pages.Repositories
{
    public class IndexModel : PageModelBase
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly ILogger<IndexModel> _logger;
        private readonly IDirectoryManager _directoryManager;
        // need to create the Repository object in handler using the repo path
        // and the Repository class does not support configuration after creation
        // does not know how to do this in a built in container
        // so we create it manually and register it for disposal

        public IndexModel(
            ApplicationDbContext applicationDbContext,
            ILogger<IndexModel> logger,
            IDirectoryManager directoryManager)
        {
            _appDbContext = applicationDbContext;
            _logger = logger;
            _directoryManager = directoryManager;
        }


        public async Task<IActionResult> OnGetAsync(string owner, string reponame)
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
                IEnumerable<string> dirs, files;
                _directoryManager.GetRelativeDirsAndFiles(RepoInfo.RootPath, RepoInfo.RootPath, out dirs, out files);
                InitHeaderSpecVM();
                InitBranchVM();
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




    }
}