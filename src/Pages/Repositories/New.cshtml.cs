using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

using BitHub.Data;
using BitHub.Services;
using BitHub.Models.Repository;

// <---------TODO--------->
// - Add README file generation logic

namespace BitHub.Pages.Repositories
{
    [Authorize(policy: "SignedIn")]
    public class NewModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IDirectoryManager _directoryManager;
        private readonly IFileManager _fileManager;
        private readonly ILogger<NewModel> _logger;


        [BindProperty]
        public RepositoryInfoModel Input { get; set; }

        [BindProperty]
        public bool IfGenerateReadme { get; set; }


        public NewModel(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext applicationDbContext,
            IDirectoryManager directoryManager,
            IFileManager fileManager,
            ILogger<NewModel> logger)
        {
            _userManager = userManager;
            _applicationDbContext = applicationDbContext;
            _directoryManager = directoryManager;
            _fileManager = fileManager;
            _logger = logger;
        }



        public async Task<IActionResult> OnPostAsync()
        {
            // assume user is signed in
            if (!ModelState.IsValid)
                return Page();

            Input.Owner = _userManager.GetUserName(User);
            var appUser = await _userManager.GetUserAsync(User);

            string repoDir;
            if((repoDir = CreateRepoRootDir(appUser.UserRootDirectory, Input.RepoName)) == null)
            {
                ModelState.AddModelError(string.Empty, "Cannot create repository due to some internal errors.");
                return Page();
            }

            Input.RootPath = repoDir;

            //////////////////////////////////
            ///  TODO : should be redirect to repo page after creation

            _applicationDbContext.Repositories.Add(Input);
            await _applicationDbContext.SaveChangesAsync();

            return RedirectToPage("/Index");

        }



        private string CreateRepoRootDir(string usrRootDir, string reponame)
        {
            // by project convention, the root of all user repos is called "Repositories"

            string repoRootDir = Path.Combine(usrRootDir, "Repositories");

            // user do not have a repo root yet, create one first
            if (!_directoryManager.Exists(repoRootDir))
            {
                try
                {
                    _directoryManager.CreateDirectory(repoRootDir);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed creating root repo directory \"{usrRootDir}\". Exception: \"{ex.Message}\"");
                    return null;
                }
            }

            string repoDir = Path.Combine(repoRootDir, reponame);

            if(!_directoryManager.Exists(repoDir))
            {
                try
                {
                    _directoryManager.CreateDirectory(repoDir);
                }
                catch(Exception ex)
                {
                    _logger.LogError($"Failed creating repo directory \"{repoDir}\". Exception: \"{ex.Message}\"");
                    return null;
                }
            }

            return repoDir;
        }
    }
}