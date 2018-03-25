using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitHub.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BitHub.Controllers
{
    [Route("[controller]/[action]")]
    public class SignUpCheckController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SignUpCheckController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> VerifyUsernameDuplicate()
        {
            string Username = HttpContext.Request.Query["Input.Username"];
            if ((await _userManager.FindByNameAsync(Username)) != null)
                return Json($"Username \"{Username}\" is already in use.");

            return Json(true);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> VerifyEmailDuplicate()
        {
            string Email = HttpContext.Request.Query["Input.Email"];

            if ((await _userManager.FindByEmailAsync(Email)) != null)
                return Json($"E-mail address \"{Email}\" is already in use.");

            return Json(true);
        }

    }
}