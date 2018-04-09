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
using BitHub.Helpers.Repository;
using BitHub.Models.Repository;
using BitHub.Extensions;
using LibGit2Sharp;

namespace BitHub.Pages.Repositories
{
    public class FileModel : PageModelBase
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly ILogger<IndexModel> _logger;
        private readonly IFileManager _fileManager;
        private readonly IFileInfoManager _fileInfoManager;



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