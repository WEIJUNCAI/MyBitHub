using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BitHub.Models.Repository;

namespace BitHub.Pages.Repositories.Components.Header
{
    public class DirectoryTableViewComponent : ViewComponent
    {

        public async Task<IViewComponentResult> InvokeAsync(RepositoryInfoModel model)
        {
            return View(model);
        }

    }
}
