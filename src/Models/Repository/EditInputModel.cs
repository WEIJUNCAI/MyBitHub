using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BitHub.Models.Repository
{
    public class EditInputModel
    {
        public string Content { get; set; }

        [RegularExpression(@"^[0-9a-zA-Z_\-. ]+$", ErrorMessage = "Invaid file name")]
        public string FileName { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        [RegularExpression(@"^(?!.*/\.)(?!.*\.\.)(?!/)(?!.*//)(?!.*@\{)(?!@$)(?!.*\\)[^\000-\037\177 ~^:?*[]+/[^\000-\037\177 ~^:?*[]+(?<!\.lock)(?<!/)(?<!\.)$", 
            ErrorMessage = "Invalid branch name")]
        public string NewBranchName { get; set; }

        public int CreateNewBranch { get; set; }
    }

    // The view model for radio buttons.
    // in ASP.NET core, a set of radio buttons are each assigned 
    // a radio button VM object that binds to "value" attribute.

    // And all of their "asp-for" tag helper field is populated by the same
    // form input VM model property which will be initialized using the VM object
    // of the checked radio button.

    public class RadioSelection
    {
        public int Id { get; set; }
    }
}
