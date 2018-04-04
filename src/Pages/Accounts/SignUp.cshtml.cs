using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using BitHub.Data;
using BitHub.Services;
using BitHub.Models.Accounts;
using BitHub.Options;

namespace BitHub.Pages.Accounts
{
    public class SignUpModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IOptions<FileDirectoryOptions> _fileDirectoryOptions;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SignInModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IDirectoryManager _directoryManager;



        [BindProperty]
        public SignUpInputModel Input { get; set; }

        public string ReturnUrl { get; set; }


        public SignUpModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<SignInModel> logger,
            IEmailSender emailSender,
            IDirectoryManager dirManager,
            IOptions<FileDirectoryOptions> fileDirectoryOptions
)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _fileDirectoryOptions = fileDirectoryOptions;
            _directoryManager = dirManager;
        }



        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }



        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                string usrRootDir;

                if ((usrRootDir = CreateUserRootDir(Input.Username)) == null)
                {
                    ModelState.AddModelError(string.Empty, "Cannot create user account due to some internal errors.");
                    return Page();
                }
                var user = new ApplicationUser { UserName = Input.Username, Email = Input.Email, UserRootDirectory = usrRootDir };

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    await _emailSender.SendEmailConfirmationAsync(Input.Email, callbackUrl);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(Url.GetLocalUrl(returnUrl));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private string CreateUserRootDir(string username)
        {
            string[] paths = { _fileDirectoryOptions.Value.AppRootDirectory, username };
            string userRootDir = Path.Combine(paths);

            if (_directoryManager.Exists(userRootDir))
            {
                _logger.LogError($"Failed creating directory for user \"{username}\", directory \"{userRootDir}\" already exists.");
                return null;
            }

            try
            {
                _directoryManager.CreateDirectory(userRootDir);
            }
            catch (Exception ex)
            {
                _logger.LogError($"\nFailed creating directory for user \"{username}\", exception message: \"{ex.Message}\"");
                return null;
            }

            return userRootDir;
        }
    }
}