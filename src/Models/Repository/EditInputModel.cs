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

    public class RadioSelection
    {
        public int Id { get; set; }
    }
}
